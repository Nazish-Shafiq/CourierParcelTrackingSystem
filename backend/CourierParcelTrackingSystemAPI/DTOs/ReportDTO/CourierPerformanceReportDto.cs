
namespace CourierParcelTrackingSystem.API.DTOs.ReportDTO;

public class CourierPerformanceReportDto
{
    public int CourierId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string CourierName { get; set; } = string.Empty;
    public int TotalAssignedParcels { get; set; }
    public int DeliveredParcels { get; set; }
}
