using System;
using System.Collections.Generic;

namespace CourierParcelTrackingSystem.API.Entities;

public partial class ParcelStatus
{
    public int ParcelStatusId { get; set; }

    public string StatusCode { get; set; } = null!;

    public string StatusName { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsTerminal { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Parcel> Parcels { get; set; } = new List<Parcel>();

    public virtual ICollection<StatusEvent> StatusEvents { get; set; } = new List<StatusEvent>();

    public virtual User? UpdatedByNavigation { get; set; }
}
