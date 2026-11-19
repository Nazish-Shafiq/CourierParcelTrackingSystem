namespace CourierParcelTrackingSystem.API.DTOs.CustomerDashboardDTO;

public class CustomerParcelDetailsDto
{
    public long ParcelId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public decimal TotalWeightKg { get; set; }
    public decimal? DeclaredValue { get; set; }
    public string? SpecialInstructions { get; set; }
    public string CurrentStatusName { get; set; } = string.Empty;
    public string CurrentStatusCode { get; set; } = string.Empty;
    public string? CurrentStatusDescription { get; set; }
    public string? OriginHubName { get; set; }
    public string? DestinationHubName { get; set; }
    public string? CurrentHubName { get; set; }
    public CustomerContactDto Sender { get; set; } = new();
    public CustomerContactDto? Recipient { get; set; }
    public List<CustomerTrackingEventDto> TrackingHistory { get; set; } = new();
}

public class CustomerContactDto
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
}

public class CustomerTrackingEventDto
{
    public long StatusEventId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? HubName { get; set; }
    public string? OriginHubName { get; set; }
    public string? DestinationHubName { get; set; }
    public DateTime EventAt { get; set; }
}
