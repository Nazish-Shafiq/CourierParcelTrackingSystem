
using System.ComponentModel.DataAnnotations;

namespace CourierParcelTrackingSystem.API.DTOs.HubDTO;

public class CreateHubRequestDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string HubName { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string AddressLine { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Province { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [Phone]
    [StringLength(30)]
    public string? ContactPhone { get; set; }
}
