using CourierParcelTrackingSystem.API.DTOs.UserDTO;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;

namespace CourierParcelTrackingSystem.API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    public UserService(IUserRepository repository) => _repository = repository;

    public async Task<List<UserResponseDto>> GetAllAsync() =>
        (await _repository.GetAllAsync()).Select(Map).ToList();

    public async Task<UserResponseDto?> GetByIdAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        return user is null ? null : Map(user);
    }

    private static UserResponseDto Map(Entities.User user) => new()
    {
        UserId = user.UserId,
        UserName = user.UserName,
        Email = user.Email,
        FullName = user.FullName,
        Phone = user.Phone,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        Roles = user.UserRoleUsers
            .Where(ur => ur.Role.IsActive && !ur.Role.IsDeleted)
            .Select(ur => ur.Role.RoleName).Distinct().ToList()
    };
}
