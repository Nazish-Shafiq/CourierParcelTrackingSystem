using CourierParcelTrackingSystem.API.DTOs.AuditLogDTO;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;

namespace CourierParcelTrackingSystem.API.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(
        IAuditLogRepository repository,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResponseDto<AuditLogResponseDto>> GetPagedAsync(
        AuditLogQueryDto query)
    {
        if (query.Page < 1)
            throw new ArgumentOutOfRangeException(nameof(query.Page));
        if (query.PageSize is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(query.PageSize));
        if (query.FromDate.HasValue &&
            query.ToDate.HasValue &&
            query.FromDate.Value > query.ToDate.Value)
        {
            throw new ArgumentException(
                "The start date must be earlier than or equal to the end date.");
        }

        var result = await _repository.GetPagedAsync(query);
        return new PagedResponseDto<AuditLogResponseDto>
        {
            Items = result.Items.Select(Map).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages
        };
    }

    public Task RecordAuthenticationEventAsync(
        string action,
        int? actorUserId) =>
        RecordAsync(
            actorUserId,
            action,
            "Authentication",
            actorUserId?.ToString() ?? "unknown");

    public async Task RecordAsync(
        int? actorUserId,
        string action,
        string entityName,
        string entityId,
        IReadOnlyDictionary<string, object?>? oldValues = null,
        IReadOnlyDictionary<string, object?>? newValues = null)
    {
        if (string.IsNullOrWhiteSpace(action) || action.Length > 50)
            throw new ArgumentException("Audit action must be 1 to 50 characters.", nameof(action));
        if (string.IsNullOrWhiteSpace(entityName) || entityName.Length > 150)
            throw new ArgumentException("Audit entity name must be 1 to 150 characters.", nameof(entityName));
        if (string.IsNullOrWhiteSpace(entityId) || entityId.Length > 100)
            throw new ArgumentException("Audit entity ID must be 1 to 100 characters.", nameof(entityId));

        var ipAddress = _httpContextAccessor.HttpContext?.Connection
            .RemoteIpAddress?.ToString();
        var auditLog = new AuditLog
        {
            ActorUserId = actorUserId,
            Action = action.Trim().ToUpperInvariant(),
            EntityName = entityName.Trim(),
            EntityId = entityId,
            OldValues = AuditLogValueSerializer.Serialize(oldValues),
            NewValues = AuditLogValueSerializer.Serialize(newValues),
            IpAddress = ipAddress,
            OccurredAt = DateTime.UtcNow
        };

        await _repository.AddAsync(auditLog);
    }

    private static AuditLogResponseDto Map(AuditLog log) => new()
    {
        AuditLogId = log.AuditLogId,
        ActorUserId = log.ActorUserId,
        ActorFullName = log.ActorUser?.FullName,
        ActorUserName = log.ActorUser?.UserName,
        Action = log.Action,
        EntityName = log.EntityName,
        EntityId = log.EntityId,
        OldValues = log.OldValues,
        NewValues = log.NewValues,
        IpAddress = log.IpAddress,
        OccurredAt = log.OccurredAt
    };
}
