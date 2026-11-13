using CourierParcelTrackingSystem.API.DTOs.AuthenticationDTO;
namespace CourierParcelTrackingSystem.API.Services.Interfaces;
public interface IAuthService
{
    Task<string> BootstrapAdminAsync(BootstrapAdminRequestDto request);
    Task<string> RegisterStaffAsync(RegisterStaffRequestDto request, int createdBy);
    Task<string> RegisterCustomerAsync(RegisterCustomerRequestDto request, int createdBy);
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto?> GetCurrentUserAsync(int userId);
}
