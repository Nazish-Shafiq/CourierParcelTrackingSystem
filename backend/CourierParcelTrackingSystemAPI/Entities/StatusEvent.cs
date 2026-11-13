using System;
using System.Collections.Generic;

namespace CourierParcelTrackingSystem.API.Entities;

public partial class StatusEvent
{
    public long StatusEventId { get; set; }

    public long ParcelId { get; set; }

    public int ParcelStatusId { get; set; }

    public int? HubId { get; set; }

    public int? CourierId { get; set; }

    public DateTime EventAt { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public virtual Courier? Courier { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Hub? Hub { get; set; }

    public virtual Parcel Parcel { get; set; } = null!;

    public virtual ParcelStatus ParcelStatus { get; set; } = null!;
}
