
namespace CourierParcelTrackingSystem.API.DTOs.DashboardDTO;

public class RecentAssignmentDto
{
    public long ParcelAssignmentId { get; set; }
    public long ParcelId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public int CourierId { get; set; }
    public string CourierName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public string? AssignmentNotes { get; set; }
}