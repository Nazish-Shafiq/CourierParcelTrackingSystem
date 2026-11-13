using System;
using System.Collections.Generic;

namespace CourierParcelTrackingSystem.API.Entities;

public partial class Route
{
    public int RouteId { get; set; }

    public string RouteCode { get; set; } = null!;

    public string RouteName { get; set; } = null!;

    public int OriginHubId { get; set; }

    public int DestinationHubId { get; set; }

    public decimal? DistanceKm { get; set; }

    public decimal? EstimatedHours { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Hub DestinationHub { get; set; } = null!;

    public virtual Hub OriginHub { get; set; } = null!;

    public virtual ICollection<Parcel> Parcels { get; set; } = new List<Parcel>();

    public virtual User? UpdatedByNavigation { get; set; }
}
