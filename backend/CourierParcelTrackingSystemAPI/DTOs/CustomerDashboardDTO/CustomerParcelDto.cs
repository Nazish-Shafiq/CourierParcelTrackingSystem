namespace CourierParcelTrackingSystem.API.DTOs.CustomerDashboardDTO;

public class CustomerParcelDto
{
    public long ParcelId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string? ReceiverName { get; set; }
    public DateTime BookingDate { get; set; }
    public string? OriginHubName { get; set; }
    public string? DestinationHubName { get; set; }
    public decimal TotalWeightKg { get; set; }
    public string CurrentStatusName { get; set; } = string.Empty;
    public string CurrentStatusCode { get; set; } = string.Empty;
    public DateTime? ExpectedDeliveryDate { get; set; }
}
