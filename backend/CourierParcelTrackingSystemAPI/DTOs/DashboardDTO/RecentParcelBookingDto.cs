
namespace CourierParcelTrackingSystem.API.DTOs.DashboardDTO;

public class RecentParcelBookingDto
{
    public long ParcelId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
}
