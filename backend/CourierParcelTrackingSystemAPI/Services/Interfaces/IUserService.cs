using CourierParcelTrackingSystem.API.DTOs.UserDTO;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface IUserService
{
    Task<List<UserResponseDto>> GetAllAsync();
    Task<UserResponseDto?> GetByIdAsync(int id);
    Task<UserResponseDto?> UpdateAsync(
        int id,
        UpdateUserRequestDto request,
        int updatedBy);
    Task<bool> SoftDeleteAsync(int id, int deletedBy);
}
