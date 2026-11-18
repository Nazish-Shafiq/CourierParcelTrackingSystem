namespace CourierParcelTrackingSystem.API.DTOs.HubTransferDTO;

public class HubTransferResponseDto
{
    public long HubTransferId { get; set; }
    public long ParcelId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public int SourceHubId { get; set; }
    public string SourceHubName { get; set; } = string.Empty;
    public int DestinationHubId { get; set; }
    public string DestinationHubName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime TransferredAt { get; set; }
    public DateTime? ArrivedAt { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}
