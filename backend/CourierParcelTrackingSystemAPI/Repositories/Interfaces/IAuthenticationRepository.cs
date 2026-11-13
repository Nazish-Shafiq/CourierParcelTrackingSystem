using CourierParcelTrackingSystem.API.Entities;
namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;
public interface IAuthRepository
{
    Task<User?> GetUserForLoginAsync(string usernameOrEmail);
    Task<User?> GetUserWithRolesAsync(int userId);
    Task<Role?> GetActiveRoleByIdAsync(int roleId);
    Task<bool> UserExistsAsync(string username, string email);
    Task<bool> AdminExistsAsync();
    Task CreateAccountAsync(User user, Role role, int? createdBy, Customer? customer = null);
    Task UpdatePasswordHashAsync(User user);
}
