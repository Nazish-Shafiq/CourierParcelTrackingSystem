using System;
using System.Collections.Generic;

namespace CourierParcelTrackingSystem.API.Entities;

public partial class ParcelAssignment
{
    public long ParcelAssignmentId { get; set; }

    public long ParcelId { get; set; }

    public int CourierId { get; set; }

    public DateTime AssignedAt { get; set; }

    public int? AssignedBy { get; set; }

    public DateTime? UnassignedAt { get; set; }

    public int? UnassignedBy { get; set; }

    public string? AssignmentNotes { get; set; }

    public virtual User? AssignedByNavigation { get; set; }

    public virtual Courier Courier { get; set; } = null!;

    public virtual Parcel Parcel { get; set; } = null!;

    public virtual User? UnassignedByNavigation { get; set; }
}
