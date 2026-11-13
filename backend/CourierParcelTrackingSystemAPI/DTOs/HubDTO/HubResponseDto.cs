namespace CourierParcelTrackingSystem.API.DTOs.HubDTO;

public class HubResponseDto
{
    public int HubId { get; set; }
    public string HubCode { get; set; } = string.Empty;
    public string HubName { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
