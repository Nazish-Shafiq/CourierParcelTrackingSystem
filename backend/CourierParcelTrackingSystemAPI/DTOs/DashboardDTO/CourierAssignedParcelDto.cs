namespace CourierParcelTrackingSystem.API.DTOs.DashboardDTO;

public class CourierAssignedParcelDto
{
    public long ParcelAssignmentId { get; set; }
    public long ParcelId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;

    public string SenderName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;

    public string RecipientCity { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;

    public DateTime AssignedAt { get; set; }
}