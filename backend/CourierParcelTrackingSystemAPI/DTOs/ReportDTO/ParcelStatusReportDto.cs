
namespace CourierParcelTrackingSystem.API.DTOs.ReportDTO;

public class ParcelStatusReportDto
{
    public int ParcelStatusId { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public int TotalParcels { get; set; }
}
