using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IScreenAssignmentRepository
{
    Task<List<Screen>> GetActiveScreensAsync();
    Task<List<User>> GetEligibleUsersAsync();
    Task<List<UserScreenPermission>> GetAssignmentsAsync();
    Task<List<UserScreenPermission>> GetUserAssignmentsAsync(int userId);
    Task<Screen?> GetActiveScreenAsync(int screenId);
    Task<User?> GetEligibleUserAsync(int userId);
    Task<UserScreenPermission?> GetAssignmentAsync(long permissionId);
    Task<bool> HasActiveAssignmentAsync(int userId, int screenId);
    Task<List<string>> GetActiveAssignmentRoutePathsAsync(
        int userId,
        IReadOnlyCollection<string> screenRoutePaths);
    Task AddAssignmentAsync(UserScreenPermission permission);
    Task ReassignAsync(UserScreenPermission current, int userId, int assignedBy);
    Task RevokeAsync(UserScreenPermission permission, int revokedBy);
}
