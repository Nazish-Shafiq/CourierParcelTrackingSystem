namespace CourierParcelTrackingSystem.API.DTOs.ReportDTO;

public class HubParcelReportDto
{
    public int HubId { get; set; }
    public string HubName { get; set; } = string.Empty;
    public string HubCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Province { get; set; }
    public bool IsActive { get; set; }
    public int OriginParcelCount { get; set; }
    public int DestinationParcelCount { get; set; }
}
