namespace CourierParcelTrackingSystem.API.DTOs.CouriersDTO;

public class CourierDispatchOptionDto
{
    public int CourierId { get; set; }

    public int HubId { get; set; }

    public string HubName { get; set; } = string.Empty;

    public string EmployeeCode { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? VehicleType { get; set; }

    public bool IsAvailable { get; set; }
}
