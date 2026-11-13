
namespace CourierParcelTrackingSystem.API.DTOs.DashboardDTO;

public class DispatcherDashboardResponseDto
{
    public int TotalParcels { get; set; }
    public int UnassignedParcels { get; set; }
    public int ActiveAssignments { get; set; }
    public int ParcelsInTransit { get; set; }
    public int AvailableCouriers { get; set; }
    public int BusyCouriers { get; set; }

    public List<RecentAssignmentDto> RecentAssignments { get; set; }
        = new();

    public List<UnassignedParcelDto> RecentUnassignedParcels { get; set; }
        = new();
}
