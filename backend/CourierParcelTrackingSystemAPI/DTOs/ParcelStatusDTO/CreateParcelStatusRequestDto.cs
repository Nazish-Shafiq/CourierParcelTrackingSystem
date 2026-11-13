
using System.ComponentModel.DataAnnotations;

namespace CourierParcelTrackingSystem.API.DTOs.ParcelStatusDTO;

public class CreateParcelStatusRequestDto
{
    [Required]
    [StringLength(100)]
    public string StatusName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; }

    public bool IsTerminal { get; set; }
}
