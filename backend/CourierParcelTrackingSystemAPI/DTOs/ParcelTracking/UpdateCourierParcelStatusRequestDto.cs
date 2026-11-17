using System.ComponentModel.DataAnnotations;

namespace CourierParcelTrackingSystem.API.DTOs.ParcelTracking;

public class UpdateCourierParcelStatusRequestDto
{
    [Required]
    [MaxLength(40)]
    public string StatusCode { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
