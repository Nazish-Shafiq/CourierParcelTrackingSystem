
namespace CourierParcelTrackingSystem.API.DTOs.ReportDTO;

public class CourierPerformanceReportDto
{
    public int CourierId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string CourierName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? HubName { get; set; }
    public string? VehicleType { get; set; }
    public string? VehicleNumber { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsActive { get; set; }
    public int TotalAssignedParcels { get; set; }
    public int ActiveAssignments { get; set; }
    public int DeliveredParcels { get; set; }
}
