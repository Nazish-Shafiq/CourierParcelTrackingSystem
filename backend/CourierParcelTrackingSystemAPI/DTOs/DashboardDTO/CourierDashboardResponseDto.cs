namespace CourierParcelTrackingSystem.API.DTOs.DashboardDTO;

public class CourierDashboardResponseDto
{
    public string CourierName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;

    public int TotalAssignedParcels { get; set; }
    public int ParcelsInTransit { get; set; }
    public int ParcelsDelivered { get; set; }

    public List<CourierAssignedParcelDto> AssignedParcels { get; set; } = new();
    public List<CourierAssignedParcelDto> DeliveryHistory { get; set; } = new();
}