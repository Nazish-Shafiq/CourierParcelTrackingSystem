using System;
using System.Collections.Generic;

namespace CourierParcelTrackingSystem.API.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<AiModel> AiModelCreatedByNavigations { get; set; } = new List<AiModel>();

    public virtual ICollection<AiModel> AiModelUpdatedByNavigations { get; set; } = new List<AiModel>();

    public virtual ICollection<AiPrompt> AiPromptCreatedByNavigations { get; set; } = new List<AiPrompt>();

    public virtual ICollection<AiPrompt> AiPromptUpdatedByNavigations { get; set; } = new List<AiPrompt>();

    public virtual ICollection<AiProvider> AiProviderCreatedByNavigations { get; set; } = new List<AiProvider>();

    public virtual ICollection<AiProvider> AiProviderUpdatedByNavigations { get; set; } = new List<AiProvider>();

    public virtual ICollection<AiRequest> AiRequests { get; set; } = new List<AiRequest>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Courier> CourierCreatedByNavigations { get; set; } = new List<Courier>();

    public virtual ICollection<Courier> CourierUpdatedByNavigations { get; set; } = new List<Courier>();

    public virtual Courier? CourierUser { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Customer> CustomerCreatedByNavigations { get; set; } = new List<Customer>();

    public virtual ICollection<Customer> CustomerUpdatedByNavigations { get; set; } = new List<Customer>();

    public virtual Customer? CustomerUser { get; set; }

    public virtual ICollection<Hub> HubCreatedByNavigations { get; set; } = new List<Hub>();

    public virtual ICollection<Hub> HubUpdatedByNavigations { get; set; } = new List<Hub>();

    public virtual ICollection<User> InverseCreatedByNavigation { get; set; } = new List<User>();

    public virtual ICollection<User> InverseUpdatedByNavigation { get; set; } = new List<User>();

    public virtual ICollection<ParcelAssignment> ParcelAssignmentAssignedByNavigations { get; set; } = new List<ParcelAssignment>();

    public virtual ICollection<ParcelAssignment> ParcelAssignmentUnassignedByNavigations { get; set; } = new List<ParcelAssignment>();

    public virtual ICollection<Parcel> ParcelCreatedByNavigations { get; set; } = new List<Parcel>();

    public virtual ICollection<ParcelRecipient> ParcelRecipientCreatedByNavigations { get; set; } = new List<ParcelRecipient>();

    public virtual ICollection<ParcelRecipient> ParcelRecipientUpdatedByNavigations { get; set; } = new List<ParcelRecipient>();

    public virtual ICollection<ParcelStatus> ParcelStatusCreatedByNavigations { get; set; } = new List<ParcelStatus>();

    public virtual ICollection<ParcelStatus> ParcelStatusUpdatedByNavigations { get; set; } = new List<ParcelStatus>();

    public virtual ICollection<Parcel> ParcelUpdatedByNavigations { get; set; } = new List<Parcel>();

    public virtual ICollection<Role> RoleCreatedByNavigations { get; set; } = new List<Role>();

    public virtual ICollection<Role> RoleUpdatedByNavigations { get; set; } = new List<Role>();

    public virtual ICollection<Route> RouteCreatedByNavigations { get; set; } = new List<Route>();

    public virtual ICollection<Route> RouteUpdatedByNavigations { get; set; } = new List<Route>();

    public virtual ICollection<Screen> ScreenCreatedByNavigations { get; set; } = new List<Screen>();

    public virtual ICollection<Screen> ScreenUpdatedByNavigations { get; set; } = new List<Screen>();

    public virtual ICollection<StatusEvent> StatusEvents { get; set; } = new List<StatusEvent>();

    public virtual User? UpdatedByNavigation { get; set; }

    public virtual ICollection<UserRole> UserRoleAssignedByNavigations { get; set; } = new List<UserRole>();

    public virtual ICollection<UserRole> UserRoleUsers { get; set; } = new List<UserRole>();

    public virtual ICollection<UserScreenPermission> UserScreenPermissionAssignedByNavigations { get; set; } = new List<UserScreenPermission>();

    public virtual ICollection<UserScreenPermission> UserScreenPermissionRevokedByNavigations { get; set; } = new List<UserScreenPermission>();

    public virtual ICollection<UserScreenPermission> UserScreenPermissionUsers { get; set; } = new List<UserScreenPermission>();
}
