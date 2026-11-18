namespace CourierParcelTrackingSystem.API.DTOs.CustomerDashboardDTO;

public class CustomerDashboardSummaryDto
{
    public int TotalParcels { get; set; }
    public int BookedParcels { get; set; }
    public int DeliveredParcels { get; set; }
}
