namespace CourierParcelTrackingSystem.API.DTOs.ParcelDTO;

public class ParcelResponseDto
{
    public long ParcelId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public int SenderCustomerId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public int? RouteId { get; set; }
    public string? RouteName { get; set; }
    public int CurrentStatusId { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public decimal TotalWeightKg { get; set; }
    public decimal? DeclaredValue { get; set; }
    public string? SpecialInstructions { get; set; }
}
