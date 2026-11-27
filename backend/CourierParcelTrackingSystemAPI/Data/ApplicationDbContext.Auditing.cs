using System.Security.Claims;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace CourierParcelTrackingSystem.API.Data;

public partial class ApplicationDbContext
{
    private static readonly HashSet<string> AuditedEntityNames = new(
        StringComparer.Ordinal)
    {
        "User",
        "UserRole",
        "Customer",
        "Courier",
        "Hub",
        "HubTransfer",
        "Route",
        "ParcelStatus",
        "UserScreenPermission",
        "Parcel",
        "ParcelRecipient",
        "ParcelAssignment",
        "Role",
        "Screen",
        "AiModel",
        "AiPrompt",
        "AiProvider"
    };

    public override int SaveChanges() =>
        SaveChanges(acceptAllChangesOnSuccess: true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        SaveChangesAsync(acceptAllChangesOnSuccess)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default) =>
        SaveChangesAsync(
            acceptAllChangesOnSuccess: true,
            cancellationToken);

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ChangeTracker.DetectChanges();
        var pendingEntries = ChangeTracker.Entries()
            .Where(entry => entry.Entity is not AuditLog &&
                entry.State is EntityState.Added or
                    EntityState.Modified or
                    EntityState.Deleted)
            .ToList();
        var stateSnapshots = pendingEntries
            .Select(CaptureState)
            .ToList();
        var auditChanges = pendingEntries
            .Where(entry => AuditedEntityNames.Contains(
                entry.Metadata.ClrType.Name))
            .Select(CreateAuditChange)
            .Where(change => change is not null)
            .Cast<PendingAuditChange>()
            .ToList();

        if (auditChanges.Count == 0)
        {
            return await base.SaveChangesAsync(
                acceptAllChangesOnSuccess,
                cancellationToken);
        }

        IDbContextTransaction? transaction = null;
        var auditLogs = new List<AuditLog>(auditChanges.Count);
        try
        {
            if (Database.IsRelational() && Database.CurrentTransaction is null)
            {
                transaction = await Database.BeginTransactionAsync(
                    cancellationToken);
            }

            var affectedRows = await base.SaveChangesAsync(
                acceptAllChangesOnSuccess: false,
                cancellationToken);

            foreach (var change in auditChanges)
                auditLogs.Add(CreateAuditLog(change));

            foreach (var snapshot in stateSnapshots)
                snapshot.Entry.State = EntityState.Unchanged;

            AuditLogs.AddRange(auditLogs);
            await base.SaveChangesAsync(
                acceptAllChangesOnSuccess: false,
                cancellationToken);

            foreach (var auditLog in auditLogs)
                Entry(auditLog).State = EntityState.Unchanged;

            if (transaction is not null)
                await transaction.CommitAsync(cancellationToken);

            if (acceptAllChangesOnSuccess)
                ChangeTracker.AcceptAllChanges();
            else
                RestoreStates(stateSnapshots);

            return affectedRows;
        }
        catch
        {
            foreach (var auditLog in auditLogs)
            {
                var entry = Entry(auditLog);
                if (entry.State != EntityState.Detached)
                    entry.State = EntityState.Detached;
            }

            RestoreStates(stateSnapshots);
            if (transaction is not null)
                await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    private static EntryStateSnapshot CaptureState(EntityEntry entry) =>
        new(
            entry,
            entry.State,
            entry.Properties
                .Where(property => property.IsModified)
                .Select(property => property.Metadata.Name)
                .ToHashSet(StringComparer.Ordinal));

    private static PendingAuditChange? CreateAuditChange(EntityEntry entry)
    {
        var state = entry.State;
        var propertyNames = state switch
        {
            EntityState.Added => entry.Properties
                .Select(property => property.Metadata.Name)
                .Where(name => !AuditLogValueSerializer.IsSensitiveProperty(name))
                .ToArray(),
            EntityState.Modified => entry.Properties
                .Where(property => property.IsModified &&
                    !AuditLogValueSerializer.IsSensitiveProperty(
                        property.Metadata.Name))
                .Select(property => property.Metadata.Name)
                .ToArray(),
            EntityState.Deleted => entry.Properties
                .Select(property => property.Metadata.Name)
                .Where(name => !AuditLogValueSerializer.IsSensitiveProperty(name))
                .ToArray(),
            _ => Array.Empty<string>()
        };

        if (propertyNames.Length == 0)
            return null;

        var oldValues = state is EntityState.Modified or EntityState.Deleted
            ? propertyNames.ToDictionary(
                name => name,
                name => entry.Property(name).OriginalValue)
            : null;

        return new PendingAuditChange(
            entry,
            state,
            propertyNames,
            oldValues,
            GetAdditionalNewValues(entry, propertyNames),
            GetAction(entry));
    }

    private AuditLog CreateAuditLog(PendingAuditChange change)
    {
        var entityId = GetEntityId(change.Entry);
        var newValues = change.State is EntityState.Added or EntityState.Modified
            ? change.PropertyNames.ToDictionary(
                name => name,
                name => change.Entry.Property(name).CurrentValue)
            : null;
        if (change.AdditionalNewValues is not null)
        {
            newValues ??= new Dictionary<string, object?>();
            foreach (var value in change.AdditionalNewValues)
                newValues[value.Key] = value.Value;
        }
        var userIdValue = _httpContextAccessor?.HttpContext?.User
            .FindFirstValue(ClaimTypes.NameIdentifier);
        var actorUserId = int.TryParse(userIdValue, out var userId)
            ? userId
            : (int?)null;

        return new AuditLog
        {
            ActorUserId = actorUserId,
            Action = change.Action,
            EntityName = change.Entry.Metadata.ClrType.Name,
            EntityId = entityId,
            OldValues = AuditLogValueSerializer.Serialize(change.OldValues),
            NewValues = AuditLogValueSerializer.Serialize(newValues),
            IpAddress = _httpContextAccessor?.HttpContext?.Connection
                .RemoteIpAddress?.ToString(),
            OccurredAt = DateTime.UtcNow
        };
    }

    private static string GetEntityId(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey()
            ?? throw new InvalidOperationException(
                $"Audited entity {entry.Metadata.ClrType.Name} has no primary key.");
        var values = key.Properties.Select(property =>
            entry.Property(property.Name).CurrentValue?.ToString()
            ?? string.Empty);
        var entityId = string.Join(",", values);

        if (string.IsNullOrWhiteSpace(entityId) || entityId.Length > 100)
        {
            throw new InvalidOperationException(
                $"Audited entity {entry.Metadata.ClrType.Name} has an invalid key value.");
        }

        return entityId;
    }

    private static string GetAction(EntityEntry entry)
    {
        var name = entry.Metadata.ClrType.Name;
        if (entry.State == EntityState.Added)
        {
            return name switch
            {
                "UserScreenPermission" => "ASSIGN",
                "ParcelAssignment" => "ASSIGN",
                "UserRole" => "ROLE_ASSIGN",
                "HubTransfer" => "HUB_TRANSFER",
                _ => "CREATE"
            };
        }

        if (entry.State == EntityState.Deleted)
        {
            return name switch
            {
                "UserScreenPermission" => "REVOKE",
                "UserRole" => "ROLE_REMOVE",
                _ => "DELETE"
            };
        }

        if (IsSoftDelete(entry))
            return "DELETE";

        if (name == "UserScreenPermission" &&
            HasChangedToFalse(entry, nameof(UserScreenPermission.IsActive)))
        {
            var permission = (UserScreenPermission)entry.Entity;
            var reassigned = entry.Context.ChangeTracker
                .Entries<UserScreenPermission>()
                .Any(candidate =>
                    candidate.State == EntityState.Added &&
                    candidate.Entity.ScreenId == permission.ScreenId);
            return reassigned ? "REASSIGN" : "REVOKE";
        }

        if (name == "ParcelAssignment" &&
            HasChangedToValue(entry, nameof(ParcelAssignment.UnassignedAt)))
        {
            var assignment = (ParcelAssignment)entry.Entity;
            var reassigned = entry.Context.ChangeTracker
                .Entries<ParcelAssignment>()
                .Any(candidate =>
                    candidate.State == EntityState.Added &&
                    candidate.Entity.ParcelId == assignment.ParcelId);
            return reassigned ? "REASSIGN" : "UNASSIGN";
        }

        if (name == "Parcel" &&
            entry.Property(nameof(Parcel.CurrentStatusId)).IsModified)
        {
            return "STATUS_CHANGE";
        }

        if (name == "Parcel" &&
            entry.Property(nameof(Parcel.RouteId)).IsModified)
        {
            return "ROUTE_ASSIGN";
        }

        if (name == "Parcel" &&
            entry.Property(nameof(Parcel.CurrentHubId)).IsModified)
        {
            return ((Parcel)entry.Entity).CurrentHubId.HasValue
                ? "HUB_ARRIVAL"
                : "HUB_DEPARTURE";
        }

        return "UPDATE";
    }

    private static Dictionary<string, object?>? GetAdditionalNewValues(
        EntityEntry entry,
        IReadOnlyCollection<string> propertyNames)
    {
        if (entry.Entity is not Parcel parcel ||
            !propertyNames.Contains(nameof(Parcel.CurrentStatusId)))
        {
            return null;
        }

        var statusEvent = entry.Context.ChangeTracker
            .Entries<StatusEvent>()
            .FirstOrDefault(candidate =>
                candidate.State == EntityState.Added &&
                candidate.Entity.ParcelId == parcel.ParcelId);
        return statusEvent is null
            ? null
            : new Dictionary<string, object?>
            {
                ["StatusEventNotes"] = statusEvent.Entity.Notes
            };
    }

    private static bool IsSoftDelete(EntityEntry entry)
    {
        var deletedProperty = entry.Metadata.FindProperty("IsDeleted");
        return deletedProperty is not null &&
            entry.Property(deletedProperty.Name).IsModified &&
            entry.Property(deletedProperty.Name).CurrentValue is true;
    }

    private static bool HasChangedToFalse(EntityEntry entry, string propertyName)
    {
        var property = entry.Metadata.FindProperty(propertyName);
        return property is not null &&
            entry.Property(propertyName).IsModified &&
            entry.Property(propertyName).CurrentValue is false;
    }

    private static bool HasChangedToValue(EntityEntry entry, string propertyName)
    {
        var property = entry.Metadata.FindProperty(propertyName);
        return property is not null &&
            entry.Property(propertyName).IsModified &&
            entry.Property(propertyName).CurrentValue is not null;
    }

    private static void RestoreStates(
        IEnumerable<EntryStateSnapshot> snapshots)
    {
        foreach (var snapshot in snapshots)
        {
            if (snapshot.Entry.State == EntityState.Detached)
                continue;

            snapshot.Entry.State = EntityState.Unchanged;
            switch (snapshot.State)
            {
                case EntityState.Added:
                    snapshot.Entry.State = EntityState.Added;
                    break;
                case EntityState.Modified:
                    foreach (var propertyName in snapshot.ModifiedProperties)
                        snapshot.Entry.Property(propertyName).IsModified = true;
                    break;
                case EntityState.Deleted:
                    snapshot.Entry.State = EntityState.Deleted;
                    break;
            }
        }
    }

    private sealed record PendingAuditChange(
        EntityEntry Entry,
        EntityState State,
        string[] PropertyNames,
        Dictionary<string, object?>? OldValues,
        Dictionary<string, object?>? AdditionalNewValues,
        string Action);

    private sealed record EntryStateSnapshot(
        EntityEntry Entry,
        EntityState State,
        HashSet<string> ModifiedProperties);
}
