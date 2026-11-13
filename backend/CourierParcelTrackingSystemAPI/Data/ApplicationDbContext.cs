
using System;
using System.Collections.Generic;
using CourierParcelTrackingSystem.API.Entities;
using Microsoft.EntityFrameworkCore;
using RouteEntity = CourierParcelTrackingSystem.API.Entities.Route;

namespace CourierParcelTrackingSystem.API.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AiModel> AiModels { get; set; }
    public virtual DbSet<AiPrompt> AiPrompts { get; set; }
    public virtual DbSet<AiProvider> AiProviders { get; set; }
    public virtual DbSet<AiRequest> AiRequests { get; set; }
    public virtual DbSet<AuditLog> AuditLogs { get; set; }
    public virtual DbSet<Courier> Couriers { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<Hub> Hubs { get; set; }
    public virtual DbSet<Parcel> Parcels { get; set; }
    public virtual DbSet<ParcelAssignment> ParcelAssignments { get; set; }
    public virtual DbSet<ParcelRecipient> ParcelRecipients { get; set; }
    public virtual DbSet<ParcelStatus> ParcelStatuses { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<RouteEntity> Routes { get; set; }
    public virtual DbSet<Screen> Screens { get; set; }
    public virtual DbSet<StatusEvent> StatusEvents { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserRole> UserRoles { get; set; }
    public virtual DbSet<UserScreenPermission> UserScreenPermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AiModel>(entity =>
        {
            entity.HasIndex(e => new { e.AiProviderId, e.ModelCode }, "UQ_AiModels_Provider_Model")
                .IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DisplayName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModelCode).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.AiProvider).WithMany(p => p.AiModels)
                .HasForeignKey(d => d.AiProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AiModels_Provider");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.AiModelCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_AiModels_CreatedBy");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.AiModelUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_AiModels_UpdatedBy");
        });

        modelBuilder.Entity<AiPrompt>(entity =>
        {
            entity.HasIndex(e => e.PromptKey, "UQ_AiPrompts_PromptKey")
                .IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModuleName).HasMaxLength(100);
            entity.Property(e => e.PromptKey).HasMaxLength(100);
            entity.Property(e => e.PromptName).HasMaxLength(150);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.AiPromptCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_AiPrompts_CreatedBy");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.AiPromptUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_AiPrompts_UpdatedBy");
        });

        modelBuilder.Entity<AiProvider>(entity =>
        {
            entity.HasIndex(e => e.ProviderCode, "UQ_AiProviders_ProviderCode")
                .IsUnique();

            entity.Property(e => e.BaseUrl).HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ProviderCode).HasMaxLength(50);
            entity.Property(e => e.ProviderName).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.AiProviderCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_AiProviders_CreatedBy");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.AiProviderUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_AiProviders_UpdatedBy");
        });

        modelBuilder.Entity<AiRequest>(entity =>
        {
            entity.HasIndex(e => new { e.ParcelId, e.RequestedAt }, "IX_AiRequests_ParcelId")
                .IsDescending(false, true);

            entity.HasIndex(e => e.RequestedAt, "IX_AiRequests_RequestedAt")
                .IsDescending();

            entity.Property(e => e.CompletedAt).HasPrecision(0);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.RequestPurpose).HasMaxLength(100);
            entity.Property(e => e.RequestStatus).HasMaxLength(30);

            entity.Property(e => e.RequestedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.AiModel).WithMany(p => p.AiRequests)
                .HasForeignKey(d => d.AiModelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AiRequests_Model");

            entity.HasOne(d => d.AiPrompt).WithMany(p => p.AiRequests)
                .HasForeignKey(d => d.AiPromptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AiRequests_Prompt");

            entity.HasOne(d => d.Parcel).WithMany(p => p.AiRequests)
                .HasForeignKey(d => d.ParcelId)
                .HasConstraintName("FK_AiRequests_Parcel");

            entity.HasOne(d => d.RequestedByNavigation).WithMany(p => p.AiRequests)
                .HasForeignKey(d => d.RequestedBy)
                .HasConstraintName("FK_AiRequests_User");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => new { e.ActorUserId, e.OccurredAt }, "IX_AuditLogs_Actor_OccurredAt")
                .IsDescending(false, true);

            entity.HasIndex(e => new { e.EntityName, e.EntityId, e.OccurredAt }, "IX_AuditLogs_Entity")
                .IsDescending(false, false, true);

            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.EntityId).HasMaxLength(100);
            entity.Property(e => e.EntityName).HasMaxLength(150);
            entity.Property(e => e.IpAddress).HasMaxLength(45);

            entity.Property(e => e.OccurredAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.ActorUser).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.ActorUserId)
                .HasConstraintName("FK_AuditLogs_Actor");
        });

        modelBuilder.Entity<Courier>(entity =>
        {
            entity.HasIndex(e => e.EmployeeCode, "UQ_Couriers_EmployeeCode")
                .IsUnique();

            entity.HasIndex(e => e.UserId, "UX_Couriers_UserId_NotNull")
                .IsUnique()
                .HasFilter("([UserId] IS NOT NULL)");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.EmployeeCode).HasMaxLength(30);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);
            entity.Property(e => e.VehicleNumber).HasMaxLength(30);
            entity.Property(e => e.VehicleType).HasMaxLength(50);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CourierCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Couriers_CreatedBy");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.CourierUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Couriers_UpdatedBy");

            entity.HasOne(d => d.User).WithOne(p => p.CourierUser)
                .HasForeignKey<Courier>(d => d.UserId)
                .HasConstraintName("FK_Couriers_User");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(e => e.CustomerCode, "UQ_Customers_CustomerCode")
                .IsUnique();

            entity.HasIndex(e => e.Email, "UX_Customers_Email_NotNull")
                .IsUnique()
                .HasFilter("([Email] IS NOT NULL)");

            entity.HasIndex(e => e.UserId, "UX_Customers_UserId_NotNull")
                .IsUnique()
                .HasFilter("([UserId] IS NOT NULL)");

            entity.Property(e => e.AddressLine).HasMaxLength(300);
            entity.Property(e => e.City).HasMaxLength(100);

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.CustomerCode).HasMaxLength(30);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CustomerCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Customers_CreatedBy");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.CustomerUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Customers_UpdatedBy");

            entity.HasOne(d => d.User).WithOne(p => p.CustomerUser)
                .HasForeignKey<Customer>(d => d.UserId)
                .HasConstraintName("FK_Customers_User");
        });

        modelBuilder.Entity<Hub>(entity =>
        {
            entity.HasIndex(e => e.HubCode, "UQ_Hubs_HubCode")
                .IsUnique();

            entity.Property(e => e.AddressLine).HasMaxLength(300);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.ContactPhone).HasMaxLength(30);
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasDefaultValue("Pakistan");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.HubCode).HasMaxLength(30);
            entity.Property(e => e.HubName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.HubCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Hubs_CreatedBy");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.HubUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Hubs_UpdatedBy");
        });

        modelBuilder.Entity<Parcel>(entity =>
        {
            entity.HasIndex(e => e.TrackingNumber, "UQ_Parcels_TrackingNumber")
                .IsUnique();

            entity.Property(e => e.BookingDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.DeclaredValue).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.DeliveredAt).HasPrecision(0);
            entity.Property(e => e.ExpectedDeliveryDate).HasPrecision(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SpecialInstructions).HasMaxLength(500);
            entity.Property(e => e.TotalWeightKg).HasColumnType("decimal(10, 3)");
            entity.Property(e => e.TrackingNumber).HasMaxLength(40);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ParcelCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Parcels_CreatedBy");

            entity.HasOne(d => d.CurrentStatus).WithMany(p => p.Parcels)
                .HasForeignKey(d => d.CurrentStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Parcels_CurrentStatus");

            entity.HasOne(d => d.Route).WithMany(p => p.Parcels)
                .HasForeignKey(d => d.RouteId)
                .HasConstraintName("FK_Parcels_Route");

            entity.HasOne(d => d.SenderCustomer).WithMany(p => p.Parcels)
                .HasForeignKey(d => d.SenderCustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Parcels_Sender");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.ParcelUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Parcels_UpdatedBy");
        });

        // Parcel Assignment configuration.
        modelBuilder.Entity<ParcelAssignment>(entity =>
        {
            entity.HasIndex(
                e => new { e.CourierId, e.UnassignedAt },
                "IX_ParcelAssignments_Courier_Active");

            // A parcel can have only one active assignment.
            // Historical assignments remain available after unassignment.
            entity.HasIndex(
                e => e.ParcelId,
                "UX_ParcelAssignments_OneActiveAssignment")
                .IsUnique()
                .HasFilter("([UnassignedAt] IS NULL)");

            entity.Property(e => e.AssignedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.AssignmentNotes).HasMaxLength(500);
            entity.Property(e => e.UnassignedAt).HasPrecision(0);

            entity.HasOne(d => d.AssignedByNavigation)
                .WithMany(p => p.ParcelAssignmentAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .HasConstraintName("FK_ParcelAssignments_AssignedBy");

            entity.HasOne(d => d.Courier)
                .WithMany(p => p.ParcelAssignments)
                .HasForeignKey(d => d.CourierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ParcelAssignments_Courier");

            // UPDATED: One parcel can have multiple historical assignments.
            entity.HasOne(d => d.Parcel)
                .WithMany(p => p.ParcelAssignments)
                .HasForeignKey(d => d.ParcelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ParcelAssignments_Parcel");

            entity.HasOne(d => d.UnassignedByNavigation)
                .WithMany(p => p.ParcelAssignmentUnassignedByNavigations)
                .HasForeignKey(d => d.UnassignedBy)
                .HasConstraintName("FK_ParcelAssignments_UnassignedBy");
        });

        modelBuilder.Entity<ParcelRecipient>(entity =>
        {
            entity.HasIndex(e => e.ParcelId, "UQ_ParcelRecipients_Parcel")
                .IsUnique();

            entity.Property(e => e.AddressLine).HasMaxLength(300);
            entity.Property(e => e.City).HasMaxLength(100);

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.RecipientName).HasMaxLength(150);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.ParcelRecipientCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_ParcelRecipients_CreatedBy");

            entity.HasOne(d => d.Parcel).WithOne(p => p.ParcelRecipient)
                .HasForeignKey<ParcelRecipient>(d => d.ParcelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ParcelRecipients_Parcel");

            entity.HasOne(d => d.UpdatedByNavigation)
                .WithMany(p => p.ParcelRecipientUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_ParcelRecipients_UpdatedBy");
        });

        modelBuilder.Entity<ParcelStatus>(entity =>
        {
            entity.HasIndex(e => e.StatusCode, "UQ_ParcelStatuses_StatusCode")
                .IsUnique();

            entity.HasIndex(e => e.StatusName, "UQ_ParcelStatuses_StatusName")
                .IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StatusCode).HasMaxLength(40);
            entity.Property(e => e.StatusName).HasMaxLength(80);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.ParcelStatusCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_ParcelStatuses_CreatedBy");

            entity.HasOne(d => d.UpdatedByNavigation)
                .WithMany(p => p.ParcelStatusUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_ParcelStatuses_UpdatedBy");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.RoleName, "UQ_Roles_RoleName")
                .IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleName).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.RoleCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Roles_CreatedBy");

            entity.HasOne(d => d.UpdatedByNavigation)
                .WithMany(p => p.RoleUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Roles_UpdatedBy");
        });

        modelBuilder.Entity<RouteEntity>(entity =>
        {
            entity.HasIndex(e => e.RouteCode, "UQ_Routes_RouteCode")
                .IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DistanceKm).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.EstimatedHours).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RouteCode).HasMaxLength(30);
            entity.Property(e => e.RouteName).HasMaxLength(150);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.RouteCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Routes_CreatedBy");

            entity.HasOne(d => d.DestinationHub)
                .WithMany(p => p.RouteDestinationHubs)
                .HasForeignKey(d => d.DestinationHubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Routes_DestinationHub");

            entity.HasOne(d => d.OriginHub)
                .WithMany(p => p.RouteOriginHubs)
                .HasForeignKey(d => d.OriginHubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Routes_OriginHub");

            entity.HasOne(d => d.UpdatedByNavigation)
                .WithMany(p => p.RouteUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Routes_UpdatedBy");
        });

        modelBuilder.Entity<Screen>(entity =>
        {
            entity.HasIndex(e => e.RoutePath, "UQ_Screens_RoutePath")
                .IsUnique();

            entity.HasIndex(e => e.ScreenKey, "UQ_Screens_ScreenKey")
                .IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModuleName).HasMaxLength(100);
            entity.Property(e => e.RoutePath).HasMaxLength(250);
            entity.Property(e => e.ScreenKey).HasMaxLength(100);
            entity.Property(e => e.ScreenName).HasMaxLength(150);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.ScreenCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Screens_CreatedBy");

            entity.HasOne(d => d.UpdatedByNavigation)
                .WithMany(p => p.ScreenUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Screens_UpdatedBy");
        });

        modelBuilder.Entity<StatusEvent>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.EventAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(d => d.Courier).WithMany(p => p.StatusEvents)
                .HasForeignKey(d => d.CourierId)
                .HasConstraintName("FK_StatusEvents_Courier");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.StatusEvents)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_StatusEvents_CreatedBy");

            entity.HasOne(d => d.Hub).WithMany(p => p.StatusEvents)
                .HasForeignKey(d => d.HubId)
                .HasConstraintName("FK_StatusEvents_Hub");

            entity.HasOne(d => d.Parcel).WithMany(p => p.StatusEvents)
                .HasForeignKey(d => d.ParcelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StatusEvents_Parcel");

            entity.HasOne(d => d.ParcelStatus).WithMany(p => p.StatusEvents)
                .HasForeignKey(d => d.ParcelStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StatusEvents_Status");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Users_Email")
                .IsUnique();

            entity.HasIndex(e => e.UserName, "UQ_Users_UserName")
                .IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);
            entity.Property(e => e.UserName).HasMaxLength(80);

            entity.HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.InverseCreatedByNavigation)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Users_CreatedBy");

            entity.HasOne(d => d.UpdatedByNavigation)
                .WithMany(p => p.InverseUpdatedByNavigation)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Users_UpdatedBy");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.Property(e => e.AssignedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.AssignedByNavigation)
                .WithMany(p => p.UserRoleAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .HasConstraintName("FK_UserRoles_AssignedBy");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Role");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoleUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_User");
        });

        modelBuilder.Entity<UserScreenPermission>(entity =>
        {
            entity.HasKey(e => e.PermissionId);

            entity.HasIndex(e => new { e.ScreenId, e.IsActive }, "IX_UserScreenPermissions_Screen_Active");

            entity.HasIndex(e => new { e.UserId, e.IsActive }, "IX_UserScreenPermissions_User_Active");

            entity.HasIndex(e => new { e.UserId, e.ScreenId }, "UX_UserScreenPermissions_Active")
                .IsUnique()
                .HasFilter("([IsActive]=(1))");

            entity.Property(e => e.AssignedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RevokedAt).HasPrecision(0);

            entity.HasOne(d => d.AssignedByNavigation)
                .WithMany(p => p.UserScreenPermissionAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserScreenPermissions_AssignedBy");

            entity.HasOne(d => d.RevokedByNavigation)
                .WithMany(p => p.UserScreenPermissionRevokedByNavigations)
                .HasForeignKey(d => d.RevokedBy)
                .HasConstraintName("FK_UserScreenPermissions_RevokedBy");

            entity.HasOne(d => d.Screen).WithMany(p => p.UserScreenPermissions)
                .HasForeignKey(d => d.ScreenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserScreenPermissions_Screen");

            entity.HasOne(d => d.User).WithMany(p => p.UserScreenPermissionUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserScreenPermissions_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
