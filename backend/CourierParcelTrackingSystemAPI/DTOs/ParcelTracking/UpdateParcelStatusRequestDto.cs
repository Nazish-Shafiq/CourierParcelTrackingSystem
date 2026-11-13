
namespace CourierParcelTrackingSystem.API.DTOs.ParcelTracking;

public class UpdateParcelStatusRequestDto
{
    public int ParcelStatusId { get; set; }

    public int? HubId { get; set; }

    public int? CourierId { get; set; }

    public string? Notes { get; set; }
}
