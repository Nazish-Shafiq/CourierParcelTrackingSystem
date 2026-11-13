
using System.ComponentModel.DataAnnotations;

namespace CourierParcelTrackingSystem.API.DTOs.CustomerDTO;

public class CreateCustomerRequestDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(256)]
    public string? Email { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 7)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(300)]
    public string? AddressLine { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? Province { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }
}
