
namespace CourierParcelTrackingSystem.API.DTOs.ParcelTracking;

public class ParcelTrackingResponseDto
{
    public long StatusEventId { get; set; }

    public long ParcelId { get; set; }

    public string TrackingNumber { get; set; } = string.Empty;

    public int ParcelStatusId { get; set; }

    public string StatusName { get; set; } = string.Empty;

    public string StatusCode { get; set; } = string.Empty;

    public int? HubId { get; set; }

    public string? HubName { get; set; }

    public int? CourierId { get; set; }

    public string? CourierName { get; set; }

    public DateTime EventAt { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
