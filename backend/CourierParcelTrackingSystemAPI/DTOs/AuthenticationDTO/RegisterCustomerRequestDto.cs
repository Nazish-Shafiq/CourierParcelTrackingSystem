using System.ComponentModel.DataAnnotations;
namespace CourierParcelTrackingSystem.API.DTOs.AuthenticationDTO;
public class RegisterCustomerRequestDto
{
    [Required, StringLength(80, MinimumLength = 3)] public string UserName { get; set; } = null!;
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = null!;
    [Required, StringLength(100, MinimumLength = 8)] public string Password { get; set; } = null!;
    [Required, StringLength(150)] public string FullName { get; set; } = null!;
    [Required, StringLength(30)] public string Phone { get; set; } = null!;
    [StringLength(300)] public string? AddressLine { get; set; }
    [StringLength(100)] public string? City { get; set; }
    [StringLength(100)] public string? Province { get; set; }
    [StringLength(20)] public string? PostalCode { get; set; }
}
