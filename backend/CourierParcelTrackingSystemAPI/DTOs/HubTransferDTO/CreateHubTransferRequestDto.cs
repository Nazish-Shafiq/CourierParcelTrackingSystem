using System.ComponentModel.DataAnnotations;

namespace CourierParcelTrackingSystem.API.DTOs.HubTransferDTO;

public class CreateHubTransferRequestDto
{
    [Required]
    public long ParcelId { get; set; }

    [Required]
    public int DestinationHubId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
