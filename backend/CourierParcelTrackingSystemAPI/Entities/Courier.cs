using System;
using System.Collections.Generic;

namespace CourierParcelTrackingSystem.API.Entities;

public partial class Courier
{
    public int CourierId { get; set; }

    public int? UserId { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public string? VehicleType { get; set; }

    public string? VehicleNumber { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<ParcelAssignment> ParcelAssignments { get; set; } = new List<ParcelAssignment>();

    public virtual ICollection<StatusEvent> StatusEvents { get; set; } = new List<StatusEvent>();

    public virtual User? UpdatedByNavigation { get; set; }

    public virtual User? User { get; set; }
}
