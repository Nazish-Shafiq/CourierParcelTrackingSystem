
namespace CourierParcelTrackingSystem.API.DTOs.DashboardDTO;

public class UnassignedParcelDto
{
    public long ParcelId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
}