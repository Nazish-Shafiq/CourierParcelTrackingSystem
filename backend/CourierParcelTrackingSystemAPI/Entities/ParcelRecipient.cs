using System;
using System.Collections.Generic;

namespace CourierParcelTrackingSystem.API.Entities;

public partial class ParcelRecipient
{
    public long ParcelRecipientId { get; set; }

    public long ParcelId { get; set; }

    public string RecipientName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public string AddressLine { get; set; } = null!;

    public string City { get; set; } = null!;

    public string? Province { get; set; }

    public string? PostalCode { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Parcel Parcel { get; set; } = null!;

    public virtual User? UpdatedByNavigation { get; set; }
}
