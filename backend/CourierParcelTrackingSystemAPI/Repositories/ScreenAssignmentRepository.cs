using CourierParcelTrackingSystem.API.Data;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierParcelTrackingSystem.API.Repositories;

public class ScreenAssignmentRepository : IScreenAssignmentRepository
{
    private static readonly string[] EligibleRoles =
    {
        "Booking Agent",
        "Dispatcher",
        "Courier"
    };

    private readonly ApplicationDbContext _context;

    public ScreenAssignmentRepository(ApplicationDbContext context) => _context = context;

    public Task<List<Screen>> GetActiveScreensAsync() =>
        _context.Screens.AsNoTracking()
            .Where(screen => screen.IsActive)
            .OrderBy(screen => screen.ModuleName)
            .ThenBy(screen => screen.ScreenName)
            .ToListAsync();

    public Task<List<User>> GetEligibleUsersAsync() =>
        _context.Users.AsNoTracking()
            .Where(user => user.IsActive && !user.IsDeleted &&
                !user.UserRoleUsers.Any(userRole =>
                    userRole.Role.IsActive &&
                    !userRole.Role.IsDeleted &&
                    userRole.Role.RoleName == "Admin") &&
                user.UserRoleUsers.Any(userRole =>
                    userRole.Role.IsActive &&
                    !userRole.Role.IsDeleted &&
                    EligibleRoles.Contains(userRole.Role.RoleName) &&
                    (userRole.Role.RoleName != "Courier" ||
                        (user.CourierUser != null &&
                         user.CourierUser.IsActive &&
                         !user.CourierUser.IsDeleted))))
            .Include(user => user.UserRoleUsers)
                .ThenInclude(userRole => userRole.Role)
            .Include(user => user.CourierUser)
            .OrderBy(user => user.FullName)
            .ToListAsync();

    public Task<List<UserScreenPermission>> GetAssignmentsAsync() =>
        _context.UserScreenPermissions.AsNoTracking()
            .Include(permission => permission.Screen)
            .Include(permission => permission.User)
                .ThenInclude(user => user.UserRoleUsers)
                    .ThenInclude(userRole => userRole.Role)
            .Include(permission => permission.User)
                .ThenInclude(user => user.CourierUser)
            .OrderByDescending(permission => permission.AssignedAt)
            .ToListAsync();

    public Task<List<UserScreenPermission>> GetUserAssignmentsAsync(int userId) =>
        _context.UserScreenPermissions.AsNoTracking()
            .Where(permission =>
                permission.UserId == userId &&
                permission.IsActive &&
                permission.User.IsActive &&
                !permission.User.IsDeleted &&
                permission.Screen.IsActive &&
                permission.User.UserRoleUsers.Any(userRole =>
                    userRole.Role.IsActive &&
                    !userRole.Role.IsDeleted &&
                    EligibleRoles.Contains(userRole.Role.RoleName) &&
                    (userRole.Role.RoleName != "Courier" ||
                        (permission.User.CourierUser != null &&
                         permission.User.CourierUser.IsActive &&
                         !permission.User.CourierUser.IsDeleted))))
            .Include(permission => permission.Screen)
            .Include(permission => permission.User)
                .ThenInclude(user => user.UserRoleUsers)
                    .ThenInclude(userRole => userRole.Role)
            .Include(permission => permission.User)
                .ThenInclude(user => user.CourierUser)
            .OrderBy(permission => permission.Screen.ModuleName)
            .ThenBy(permission => permission.Screen.ScreenName)
            .ToListAsync();

    public Task<Screen?> GetActiveScreenAsync(int screenId) =>
        _context.Screens.FirstOrDefaultAsync(screen =>
            screen.ScreenId == screenId && screen.IsActive);

    public Task<User?> GetEligibleUserAsync(int userId) =>
        _context.Users
            .Where(user =>
                user.UserId == userId &&
                user.IsActive &&
                !user.IsDeleted &&
                !user.UserRoleUsers.Any(userRole =>
                    userRole.Role.IsActive &&
                    !userRole.Role.IsDeleted &&
                    userRole.Role.RoleName == "Admin") &&
                user.UserRoleUsers.Any(userRole =>
                    userRole.Role.IsActive &&
                    !userRole.Role.IsDeleted &&
                    EligibleRoles.Contains(userRole.Role.RoleName) &&
                    (userRole.Role.RoleName != "Courier" ||
                        (user.CourierUser != null &&
                         user.CourierUser.IsActive &&
                         !user.CourierUser.IsDeleted))))
            .Include(user => user.UserRoleUsers)
                .ThenInclude(userRole => userRole.Role)
            .Include(user => user.CourierUser)
            .FirstOrDefaultAsync();

    public Task<UserScreenPermission?> GetAssignmentAsync(long permissionId) =>
        _context.UserScreenPermissions
            .Include(permission => permission.Screen)
            .Include(permission => permission.User)
                .ThenInclude(user => user.UserRoleUsers)
                    .ThenInclude(userRole => userRole.Role)
            .Include(permission => permission.User)
                .ThenInclude(user => user.CourierUser)
            .FirstOrDefaultAsync(permission => permission.PermissionId == permissionId);

    public Task<bool> HasActiveAssignmentAsync(int userId, int screenId) =>
        _context.UserScreenPermissions.AnyAsync(permission =>
            permission.UserId == userId &&
            permission.ScreenId == screenId &&
            permission.IsActive &&
            permission.User.IsActive &&
            !permission.User.IsDeleted &&
            permission.Screen.IsActive);

    public Task<List<string>> GetActiveAssignmentRoutePathsAsync(
        int userId,
        IReadOnlyCollection<string> screenRoutePaths) =>
        _context.UserScreenPermissions
            .Where(permission =>
            permission.UserId == userId &&
            permission.IsActive &&
            permission.User.IsActive &&
            !permission.User.IsDeleted &&
            permission.Screen.IsActive &&
            screenRoutePaths.Contains(permission.Screen.RoutePath) &&
            (!permission.Screen.RoutePath.StartsWith("/courier/") ||
                (permission.User.CourierUser != null &&
                 permission.User.CourierUser.IsActive &&
                 !permission.User.CourierUser.IsDeleted)) &&
            permission.User.UserRoleUsers.Any(userRole =>
                userRole.Role.IsActive &&
                !userRole.Role.IsDeleted &&
                EligibleRoles.Contains(userRole.Role.RoleName) &&
                (userRole.Role.RoleName != "Courier" ||
                    (permission.User.CourierUser != null &&
                     permission.User.CourierUser.IsActive &&
                     !permission.User.CourierUser.IsDeleted))))
            .Select(permission => permission.Screen.RoutePath)
            .Distinct()
            .ToListAsync();

    public async Task AddAssignmentAsync(UserScreenPermission permission)
    {
        _context.UserScreenPermissions.Add(permission);
        await _context.SaveChangesAsync();
    }

    public async Task ReassignAsync(
        UserScreenPermission current,
        int userId,
        int assignedBy)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        current.IsActive = false;
        current.RevokedAt = DateTime.UtcNow;
        current.RevokedBy = assignedBy;
        await _context.SaveChangesAsync();

        _context.UserScreenPermissions.Add(new UserScreenPermission
        {
            UserId = userId,
            ScreenId = current.ScreenId,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        });
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task RevokeAsync(UserScreenPermission permission, int revokedBy)
    {
        permission.IsActive = false;
        permission.RevokedAt = DateTime.UtcNow;
        permission.RevokedBy = revokedBy;
        await _context.SaveChangesAsync();
    }
}
