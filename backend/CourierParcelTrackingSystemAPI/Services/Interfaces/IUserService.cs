using CourierParcelTrackingSystem.API.DTOs.UserDTO;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface IUserService
{
    Task<List<UserResponseDto>> GetAllAsync();
    Task<UserResponseDto?> GetByIdAsync(int id);
}
