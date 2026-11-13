
using System.ComponentModel.DataAnnotations;
namespace CourierParcelTrackingSystem.API.DTOs.CouriersDTO;

public class CreateCourierRequestDto
{
    public int? UserId { get; set; }

    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(30, MinimumLength = 7)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(256)]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? VehicleType { get; set; }

    [StringLength(30)]
    public string? VehicleNumber { get; set; }
}
