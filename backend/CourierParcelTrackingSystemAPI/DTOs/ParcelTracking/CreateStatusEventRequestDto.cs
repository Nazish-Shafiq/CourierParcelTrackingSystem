
namespace CourierParcelTrackingSystem.API.DTOs.ParcelTracking;

public class CreateStatusEventRequestDto
{
    public long ParcelId { get; set; }

    public int ParcelStatusId { get; set; }

    public int? HubId { get; set; }

    public int? CourierId { get; set; }

    public string? Notes { get; set; }
}
