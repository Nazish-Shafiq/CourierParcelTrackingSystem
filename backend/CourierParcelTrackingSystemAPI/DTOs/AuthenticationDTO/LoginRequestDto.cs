using System.ComponentModel.DataAnnotations;
namespace CourierParcelTrackingSystem.API.DTOs.AuthenticationDTO;
public class LoginRequestDto
{
    [Required, StringLength(256)] public string UserNameOrEmail { get; set; } = null!;
    [Required] public string Password { get; set; } = null!;
}
