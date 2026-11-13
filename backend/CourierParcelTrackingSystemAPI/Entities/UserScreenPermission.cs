using System;
using System.Collections.Generic;

namespace CourierParcelTrackingSystem.API.Entities;

public partial class UserScreenPermission
{
    public long PermissionId { get; set; }

    public int UserId { get; set; }

    public int ScreenId { get; set; }

    public int AssignedBy { get; set; }

    public DateTime AssignedAt { get; set; }

    public bool IsActive { get; set; }

    public DateTime? RevokedAt { get; set; }

    public int? RevokedBy { get; set; }

    public virtual User AssignedByNavigation { get; set; } = null!;

    public virtual User? RevokedByNavigation { get; set; }

    public virtual Screen Screen { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
