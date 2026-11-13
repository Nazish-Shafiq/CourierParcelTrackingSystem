
namespace CourierParcelTrackingSystem.API.DTOs.DashboardDTO;

public class AdminDashboardResponseDto
{
    public int TotalParcels { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalCouriers { get; set; }
    public int AvailableCouriers { get; set; }
    public int BusyCouriers { get; set; }
    public int TotalHubs { get; set; }
    public int TotalRoutes { get; set; }

    public List<ParcelStatusSummaryDto> ParcelStatusSummary { get; set; }
        = new();

    public List<RecentParcelBookingDto> RecentBookings { get; set; }
        = new();
}
