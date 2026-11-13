using System;
using System.Collections.Generic;

namespace CourierParcelTrackingSystem.API.Entities;

public partial class Screen
{
    public int ScreenId { get; set; }

    public string ScreenName { get; set; } = null!;

    public string ScreenKey { get; set; } = null!;

    public string ModuleName { get; set; } = null!;

    public string RoutePath { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }

    public virtual ICollection<UserScreenPermission> UserScreenPermissions { get; set; } = new List<UserScreenPermission>();
}
