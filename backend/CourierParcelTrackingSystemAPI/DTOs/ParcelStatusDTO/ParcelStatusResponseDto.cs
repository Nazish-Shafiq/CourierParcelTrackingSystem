
namespace CourierParcelTrackingSystem.API.DTOs.ParcelStatusDTO;

public class ParcelStatusResponseDto
{
    public int ParcelStatusId { get; set; }

    public string StatusCode { get; set; } = string.Empty;

    public string StatusName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsTerminal { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
