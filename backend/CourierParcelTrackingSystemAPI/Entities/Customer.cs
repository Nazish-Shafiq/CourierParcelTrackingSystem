using System;
using System.Collections.Generic;

namespace CourierParcelTrackingSystem.API.Entities;

public partial class Customer
{
    public int CustomerId { get; set; }

    public int? UserId { get; set; }

    public string CustomerCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string Phone { get; set; } = null!;

    public string? AddressLine { get; set; }

    public string? City { get; set; }

    public string? Province { get; set; }

    public string? PostalCode { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Parcel> Parcels { get; set; } = new List<Parcel>();

    public virtual User? UpdatedByNavigation { get; set; }

    public virtual User? User { get; set; }
}
