
namespace CourierParcelTrackingSystem.API.DTOs.DashboardDTO;

public class ParcelStatusSummaryDto
{
    public int ParcelStatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public int TotalParcels { get; set; }
}
