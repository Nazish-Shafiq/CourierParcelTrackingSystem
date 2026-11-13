
namespace CourierParcelTrackingSystem.API.DTOs.ReportDTO;

public class ParcelBookingReportDto
{
    public long ParcelId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientCity { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public decimal TotalWeightKg { get; set; }
}
