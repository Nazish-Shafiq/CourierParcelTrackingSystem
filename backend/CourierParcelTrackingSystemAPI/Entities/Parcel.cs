using System;
using System.Collections.Generic;

namespace CourierParcelTrackingSystem.API.Entities;

public partial class Parcel
{
    public long ParcelId { get; set; }

    public string TrackingNumber { get; set; } = null!;

    public int SenderCustomerId { get; set; }

    public int? RouteId { get; set; }

    public int CurrentStatusId { get; set; }

    public DateTime BookingDate { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public decimal TotalWeightKg { get; set; }

    public decimal? DeclaredValue { get; set; }

    public string? SpecialInstructions { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<AiRequest> AiRequests { get; set; } = new List<AiRequest>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ParcelStatus CurrentStatus { get; set; } = null!;

    public virtual ICollection<ParcelAssignment> ParcelAssignments { get; set; }
    = new List<ParcelAssignment>();

    public virtual ParcelRecipient? ParcelRecipient { get; set; }

    public virtual Route? Route { get; set; }

    public virtual Customer SenderCustomer { get; set; } = null!;

    public virtual ICollection<StatusEvent> StatusEvents { get; set; } = new List<StatusEvent>();

    public virtual User? UpdatedByNavigation { get; set; }
}
