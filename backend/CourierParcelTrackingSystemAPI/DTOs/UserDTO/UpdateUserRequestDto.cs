using System.ComponentModel.DataAnnotations;

namespace CourierParcelTrackingSystem.API.DTOs.UserDTO;

public class UpdateUserRequestDto
{
    [Required]
    [StringLength(80, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(30, MinimumLength = 7)]
    public string? Phone { get; set; }

    [Range(1, int.MaxValue)]
    public int RoleId { get; set; }

    public bool IsActive { get; set; }
}
