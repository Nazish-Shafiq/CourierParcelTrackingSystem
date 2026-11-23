using CourierParcelTrackingSystem.API.DTOs.ScreenAssignmentDTO;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;

namespace CourierParcelTrackingSystem.API.Services;

public class ScreenAssignmentService : IScreenAssignmentService
{
    private static readonly string[] EligibleRoles =
    {
        "Booking Agent",
        "Dispatcher",
        "Courier"
    };

    private static readonly IReadOnlyDictionary<string, string> LegacyScreenRoutePaths =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["/parcels"] = "/dispatch/parcels",
            ["/parcels/create"] = "/booking/book-parcel",
            ["/customers"] = "/booking/customers",
            ["/hubs"] = "/dispatch/hubs",
            ["/routes"] = "/dispatch/routes",
            ["/couriers"] = "/dispatch/couriers",
            ["/assignments"] = "/dispatch/assignments",
            ["/tracking"] = "/dispatch/tracking"
        };

    private static readonly HashSet<string> RetiredScreenRoutePaths =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "/dispatch/hub-transfers",
            "/hub-transfers"
        };

    private readonly IScreenAssignmentRepository _repository;

    public ScreenAssignmentService(IScreenAssignmentRepository repository) =>
        _repository = repository;

    public async Task<List<ScreenOptionDto>> GetActiveScreensAsync() =>
        (await _repository.GetActiveScreensAsync())
            .Where(screen => !RetiredScreenRoutePaths.Contains(
                NormalizeRoutePath(screen.RoutePath)))
            .Select(MapScreen)
            .ToList();

    public async Task<List<ScreenAssignmentUserDto>> GetEligibleUsersAsync(int screenId)
    {
        var screen = await _repository.GetActiveScreenAsync(screenId);
        var requiredRole = screen is null
            ? null
            : GetRequiredRole(screen.RoutePath);
        if (screen is null || requiredRole is null)
            return new List<ScreenAssignmentUserDto>();

        return (await _repository.GetEligibleUsersAsync())
            .Select(user => new
            {
                User = user,
                EligibleRoles = GetEligibleRoles(user).ToList()
            })
            .Where(candidate =>
                candidate.EligibleRoles.Count > 0 &&
                HasRequiredOperationalProfile(candidate.User, requiredRole) &&
                !candidate.EligibleRoles.Contains(
                    requiredRole,
                    StringComparer.OrdinalIgnoreCase))
            .Select(candidate => new ScreenAssignmentUserDto
            {
                UserId = candidate.User.UserId,
                UserName = candidate.User.UserName,
                FullName = candidate.User.FullName,
                RoleNames = GetActiveRoleNames(candidate.User)
            })
            .ToList();
    }

    public async Task<List<ScreenAssignmentResponseDto>> GetAssignmentsAsync() =>
        (await _repository.GetAssignmentsAsync()).Select(MapAssignment).ToList();

    public async Task<List<ScreenOptionDto>> GetMyScreensAsync(int userId) =>
        (await _repository.GetUserAssignmentsAsync(userId))
            .Where(permission =>
            {
                var requiredRole = GetRequiredRole(permission.Screen.RoutePath);
                var eligibleRoles = GetEligibleRoles(permission.User).ToList();
                return requiredRole is not null &&
                    eligibleRoles.Count > 0 &&
                    HasRequiredOperationalProfile(permission.User, requiredRole) &&
                    !eligibleRoles.Contains(
                        requiredRole,
                        StringComparer.OrdinalIgnoreCase);
            })
            .Select(permission => MapScreen(permission.Screen))
            .ToList();

    public Task<List<string>> GetActiveAssignmentRoutePathsAsync(
        int userId,
        IReadOnlyCollection<string> screenRoutePaths)
    {
        var allowedRoutePaths = screenRoutePaths
            .Select(NormalizeRoutePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var databaseRoutePaths = allowedRoutePaths
            .SelectMany(GetDatabaseRoutePaths)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return GetMappedAssignmentRoutePathsAsync(
            userId,
            databaseRoutePaths,
            allowedRoutePaths);
    }

    private async Task<List<string>> GetMappedAssignmentRoutePathsAsync(
        int userId,
        IReadOnlyCollection<string> databaseRoutePaths,
        IReadOnlySet<string> allowedRoutePaths)
    {
        if (databaseRoutePaths.Count == 0)
            return new List<string>();

        var activePaths = await _repository.GetActiveAssignmentRoutePathsAsync(
            userId,
            databaseRoutePaths);
        return activePaths
            .Select(GetOperationalRoutePath)
            .Where(routePath => routePath is not null &&
                allowedRoutePaths.Contains(routePath))
            .Select(routePath => routePath!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<ScreenAssignmentResponseDto> AssignAsync(
        CreateScreenAssignmentRequestDto request,
        int assignedBy)
    {
        var screen = await _repository.GetActiveScreenAsync(request.ScreenId)
            ?? throw new InvalidOperationException("The selected screen is unavailable.");
        var requiredRole = GetRequiredRole(screen.RoutePath);
        if (requiredRole is null)
            throw new InvalidOperationException(
                "Only active operational screens can be assigned.");

        var user = await _repository.GetEligibleUserAsync(request.UserId)
            ?? throw new InvalidOperationException(
                "Only active Booking Agent, Dispatcher, or Courier users can receive screen assignments.");
        if (!HasRequiredOperationalProfile(user, requiredRole))
            throw new InvalidOperationException(
                "Courier screen assignments require the selected user to have an active Courier profile.");
        if (GetEligibleRoles(user).Contains(
                requiredRole,
                StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "The selected user's original role already grants access to this screen.");

        if (await _repository.HasActiveAssignmentAsync(user.UserId, screen.ScreenId))
            throw new InvalidOperationException(
                "This user is already assigned to the selected screen.");

        var permission = new UserScreenPermission
        {
            UserId = user.UserId,
            ScreenId = screen.ScreenId,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            User = user,
            Screen = screen
        };
        await _repository.AddAssignmentAsync(permission);
        return MapAssignment(permission);
    }

    public async Task<ScreenAssignmentResponseDto?> ReassignAsync(
        long permissionId,
        ReassignScreenRequestDto request,
        int assignedBy)
    {
        var current = await _repository.GetAssignmentAsync(permissionId);
        if (current is null || !current.IsActive)
            return null;

        if (!current.Screen.IsActive)
            throw new InvalidOperationException(
                "Only active operational screens can be reassigned.");

        var requiredRole = GetRequiredRole(current.Screen.RoutePath);
        if (requiredRole is null)
            throw new InvalidOperationException(
                "Only active operational screens can be reassigned.");

        var user = await _repository.GetEligibleUserAsync(request.UserId)
            ?? throw new InvalidOperationException(
                "Only active Booking Agent, Dispatcher, or Courier users can receive screen assignments.");
        if (!HasRequiredOperationalProfile(user, requiredRole))
            throw new InvalidOperationException(
                "Courier screen assignments require the selected user to have an active Courier profile.");
        if (GetEligibleRoles(user).Contains(
                requiredRole,
                StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "The selected user's original role already grants access to this screen.");

        if (current.UserId == user.UserId)
            throw new InvalidOperationException(
                "The selected user is already responsible for this screen.");

        if (await _repository.HasActiveAssignmentAsync(user.UserId, current.ScreenId))
            throw new InvalidOperationException(
                "This user is already assigned to the selected screen.");

        await _repository.ReassignAsync(current, user.UserId, assignedBy);
        var updated = await _repository.GetAssignmentsAsync();
        return updated
            .Where(permission => permission.IsActive &&
                permission.ScreenId == current.ScreenId &&
                permission.UserId == user.UserId)
            .Select(MapAssignment)
            .FirstOrDefault();
    }

    public async Task<bool> RevokeAsync(long permissionId, int revokedBy)
    {
        var permission = await _repository.GetAssignmentAsync(permissionId);
        if (permission is null || !permission.IsActive)
            return false;

        await _repository.RevokeAsync(permission, revokedBy);
        return true;
    }

    private static ScreenOptionDto MapScreen(Screen screen) => new()
    {
        ScreenId = screen.ScreenId,
        ScreenName = screen.ScreenName,
        ScreenKey = screen.ScreenKey,
        ModuleName = screen.ModuleName,
        RoutePath = GetOperationalRoutePath(screen.RoutePath) ?? screen.RoutePath,
        Description = screen.Description,
        IsAssignable = GetOperationalRoutePath(screen.RoutePath) is not null
    };

    private static ScreenAssignmentResponseDto MapAssignment(
        UserScreenPermission permission)
    {
        var userIsActive = permission.User.IsActive && !permission.User.IsDeleted;
        var eligibleRoles = GetEligibleRoles(permission.User);
        var userHasEligibleRole = eligibleRoles.Any();
        var requiredRole = GetRequiredRole(permission.Screen.RoutePath);
        return new ScreenAssignmentResponseDto
        {
            PermissionId = permission.PermissionId,
            ScreenId = permission.ScreenId,
            ScreenName = permission.Screen.ScreenName,
            ScreenKey = permission.Screen.ScreenKey,
            ModuleName = permission.Screen.ModuleName,
            RoutePath = GetOperationalRoutePath(permission.Screen.RoutePath)
                ?? permission.Screen.RoutePath,
            UserId = permission.UserId,
            AssignedUserName = permission.User.UserName,
            AssignedUserFullName = permission.User.FullName,
            AssignedUserRoles = GetActiveRoleNames(permission.User),
            AssignedAt = permission.AssignedAt,
            IsActive = permission.IsActive,
            Status = !permission.IsActive
                ? "Revoked"
                : userIsActive &&
                    permission.Screen.IsActive &&
                    requiredRole is not null &&
                    HasRequiredOperationalProfile(permission.User, requiredRole) &&
                    userHasEligibleRole
                    ? "Active"
                    : "Inactive",
            RevokedAt = permission.RevokedAt
        };
    }

    private static IEnumerable<string> GetEligibleRoles(User user) =>
        user.UserRoleUsers
            .Where(userRole =>
                userRole.Role.IsActive &&
                !userRole.Role.IsDeleted &&
                EligibleRoles.Contains(
                    userRole.Role.RoleName,
                    StringComparer.OrdinalIgnoreCase) &&
                (!string.Equals(
                    userRole.Role.RoleName,
                    "Courier",
                    StringComparison.OrdinalIgnoreCase) ||
                    (user.CourierUser is not null &&
                     user.CourierUser.IsActive &&
                     !user.CourierUser.IsDeleted)))
            .Select(userRole => userRole.Role.RoleName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(role => Array.IndexOf(EligibleRoles, role));

    private static bool HasRequiredOperationalProfile(User user, string requiredRole) =>
        !string.Equals(
            requiredRole,
            "Courier",
            StringComparison.OrdinalIgnoreCase) ||
        (user.CourierUser is not null &&
         user.CourierUser.IsActive &&
         !user.CourierUser.IsDeleted);

    private static IEnumerable<string> GetDatabaseRoutePaths(string accessRoutePath)
    {
        yield return accessRoutePath;
        foreach (var (screenRoutePath, mappedRoutePath) in LegacyScreenRoutePaths)
        {
            if (string.Equals(
                    mappedRoutePath,
                    accessRoutePath,
                    StringComparison.OrdinalIgnoreCase))
                yield return screenRoutePath;
        }
    }

    private static string? GetOperationalRoutePath(string screenRoutePath)
    {
        var normalizedPath = NormalizeRoutePath(screenRoutePath);
        if (RetiredScreenRoutePaths.Contains(normalizedPath))
            return null;

        return GetRequiredRoleForRoutePath(normalizedPath) is not null
            ? normalizedPath
            : LegacyScreenRoutePaths.TryGetValue(normalizedPath, out var mappedPath)
                ? mappedPath
                : null;
    }

    private static string NormalizeRoutePath(string routePath)
    {
        var normalizedPath = routePath.Trim().Trim('/');
        return normalizedPath.Length == 0
            ? "/"
            : $"/{normalizedPath}";
    }

    private static List<string> GetActiveRoleNames(User user) =>
        user.UserRoleUsers
            .Where(userRole =>
                userRole.Role.IsActive &&
                !userRole.Role.IsDeleted)
            .Select(userRole => userRole.Role.RoleName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(role => role, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static string? GetRequiredRole(string routePath)
    {
        var operationalRoutePath = GetOperationalRoutePath(routePath);
        return operationalRoutePath is null
            ? null
            : GetRequiredRoleForRoutePath(operationalRoutePath);
    }

    private static string? GetRequiredRoleForRoutePath(string routePath)
    {
        if (routePath.StartsWith("/booking/", StringComparison.OrdinalIgnoreCase))
            return "Booking Agent";
        if (routePath.StartsWith("/dispatch/", StringComparison.OrdinalIgnoreCase))
            return "Dispatcher";
        if (routePath.StartsWith("/courier/", StringComparison.OrdinalIgnoreCase))
            return "Courier";
        return null;
    }
}
