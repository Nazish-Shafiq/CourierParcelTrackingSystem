
namespace CourierParcelTrackingSystem.API.DTOs.CouriersDTO;

public class CourierResponseDto
{
    public int CourierId { get; set; }

    public int? UserId { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? VehicleType { get; set; }

    public string? VehicleNumber { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
