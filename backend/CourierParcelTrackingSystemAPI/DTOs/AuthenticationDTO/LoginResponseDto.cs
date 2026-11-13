namespace CourierParcelTrackingSystem.API.DTOs.AuthenticationDTO;
public class LoginResponseDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
    public List<int> RoleIds { get; set; } = new();
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}
