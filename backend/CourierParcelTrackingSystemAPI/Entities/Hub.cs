using System;
using System.Collections.Generic;

namespace CourierParcelTrackingSystem.API.Entities;

public partial class Hub
{
    public int HubId { get; set; }

    public string HubCode { get; set; } = null!;

    public string HubName { get; set; } = null!;

    public string AddressLine { get; set; } = null!;

    public string City { get; set; } = null!;

    public string? Province { get; set; }

    public string? PostalCode { get; set; }

    public string Country { get; set; } = null!;

    public string? ContactPhone { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Route> RouteDestinationHubs { get; set; } = new List<Route>();

    public virtual ICollection<Route> RouteOriginHubs { get; set; } = new List<Route>();

    public virtual ICollection<StatusEvent> StatusEvents { get; set; } = new List<StatusEvent>();

    public virtual User? UpdatedByNavigation { get; set; }
}
