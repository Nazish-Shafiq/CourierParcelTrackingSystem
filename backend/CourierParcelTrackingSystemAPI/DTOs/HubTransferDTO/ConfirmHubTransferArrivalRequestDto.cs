using System.ComponentModel.DataAnnotations;

namespace CourierParcelTrackingSystem.API.DTOs.HubTransferDTO;

public class ConfirmHubTransferArrivalRequestDto
{
    [MaxLength(500)]
    public string? Notes { get; set; }
}
