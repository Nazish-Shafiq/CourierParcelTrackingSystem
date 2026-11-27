namespace CourierParcelTrackingSystem.API.DTOs.DashboardDTO;

public class CourierAssignedParcelDto
{
    public long ParcelAssignmentId { get; set; }
    public long ParcelId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;

    public string SenderName { get; set; } = string.Empty;
    public string? SenderPhone { get; set; }
    public string? SenderAddressLine { get; set; }
    public string? SenderCity { get; set; }
    public string? SenderProvince { get; set; }
    public string? SenderPostalCode { get; set; }

    public string RecipientName { get; set; } = string.Empty;
    public string? RecipientPhone { get; set; }
    public string? RecipientEmail { get; set; }
    public string? RecipientAddressLine { get; set; }
    public string RecipientCity { get; set; } = string.Empty;
    public string? RecipientProvince { get; set; }
    public string? RecipientPostalCode { get; set; }

    public decimal TotalWeightKg { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? RouteName { get; set; }
    public string? OriginHubName { get; set; }
    public string? DestinationHubName { get; set; }
    public string? CurrentHubName { get; set; }
    public string? AssignmentNotes { get; set; }

    public string CurrentStatus { get; set; } = string.Empty;
    public string CurrentStatusCode { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}