namespace CourierParcelTrackingSystem.API.DTOs.ReportDTO;

public class DeliveryReportDto
{
    public int SuccessfulDeliveries { get; set; }
    public int FailedDeliveryAttempts { get; set; }
    public List<DeliveryReportEventDto> Events { get; set; } = new();
}

public class DeliveryReportEventDto
{
    public long StatusEventId { get; set; }
    public long ParcelId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string? CourierName { get; set; }
    public string? HubName { get; set; }
    public DateTime EventAt { get; set; }
}
