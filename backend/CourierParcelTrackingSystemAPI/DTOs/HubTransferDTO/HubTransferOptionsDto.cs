namespace CourierParcelTrackingSystem.API.DTOs.HubTransferDTO;

public class HubTransferParcelOptionDto
{
    public long ParcelId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public int CurrentHubId { get; set; }
    public string CurrentHubName { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
}

public class HubTransferHubOptionDto
{
    public int HubId { get; set; }
    public string HubCode { get; set; } = string.Empty;
    public string HubName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class HubTransferOptionsDto
{
    public List<HubTransferParcelOptionDto> Parcels { get; set; } = new();
    public List<HubTransferHubOptionDto> Hubs { get; set; } = new();
}
