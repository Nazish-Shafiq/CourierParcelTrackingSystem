
namespace CourierParcelTrackingSystem.API.DTOs.CustomerDTO;

public class CustomerResponseDto
{
    public int CustomerId { get; set; }
    public int? UserId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
