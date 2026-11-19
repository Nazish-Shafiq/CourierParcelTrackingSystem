using CourierParcelTrackingSystem.API.Data;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierParcelTrackingSystem.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    public UserRepository(ApplicationDbContext context) => _context = context;

    public Task<List<User>> GetAllAsync() =>
        _context.Users.AsNoTracking()
            .Where(u => !u.IsDeleted)
            .Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role)
            .OrderBy(u => u.UserId).ToListAsync();

    public Task<User?> GetByIdAsync(int id) =>
        _context.Users
            .Where(u => u.UserId == id && !u.IsDeleted)
            .Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role)
            .Include(u => u.CourierUser).ThenInclude(courier => courier!.Hub)
            .Include(u => u.CustomerUser)
            .FirstOrDefaultAsync();

    public Task<bool> UserNameOrEmailExistsAsync(
        string userName,
        string email,
        int excludeUserId) =>
        _context.Users.AnyAsync(user =>
            user.UserId != excludeUserId &&
            (user.UserName == userName || user.Email == email));

    public Task<bool> HasOtherActiveAdminAsync(int excludeUserId) =>
        _context.Users.AnyAsync(user =>
            user.UserId != excludeUserId &&
            user.IsActive &&
            !user.IsDeleted &&
            user.UserRoleUsers.Any(userRole =>
                userRole.Role.RoleName == "Admin" &&
                userRole.Role.IsActive &&
                !userRole.Role.IsDeleted));

    public Task<bool> HasActiveCourierAssignmentAsync(int userId) =>
        _context.Couriers.AnyAsync(courier =>
            courier.UserId == userId &&
            courier.IsActive &&
            !courier.IsDeleted &&
            courier.ParcelAssignments.Any(assignment =>
                assignment.UnassignedAt == null));

    public Task<Role?> GetActiveRoleByIdAsync(int roleId) =>
        _context.Roles.FirstOrDefaultAsync(role =>
            role.RoleId == roleId && role.IsActive && !role.IsDeleted);

    public Task SetUserRoleAsync(User user, Role role, int assignedBy)
    {
        foreach (var existingRole in user.UserRoleUsers
                     .Where(userRole => userRole.RoleId != role.RoleId)
                     .ToList())
        {
            _context.UserRoles.Remove(existingRole);
            user.UserRoleUsers.Remove(existingRole);
        }

        if (!user.UserRoleUsers.Any(userRole => userRole.RoleId == role.RoleId))
        {
            user.UserRoleUsers.Add(new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                AssignedBy = assignedBy,
                AssignedAt = DateTime.UtcNow,
                User = user,
                Role = role
            });
        }

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
