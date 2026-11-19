using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<bool> UserNameOrEmailExistsAsync(
        string userName,
        string email,
        int excludeUserId);
    Task<bool> HasOtherActiveAdminAsync(int excludeUserId);
    Task<bool> HasActiveCourierAssignmentAsync(int userId);
    Task<Role?> GetActiveRoleByIdAsync(int roleId);
    Task SetUserRoleAsync(User user, Role role, int assignedBy);
    Task SaveChangesAsync();
}
