using CourierParcelTrackingSystem.API.DTOs.UserDTO;
using CourierParcelTrackingSystem.API.Entities;
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

    public async Task<UserResponseDto?> UpdateAsync(
        int id,
        UpdateUserRequestDto request,
        int updatedBy)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            return null;

        var role = await _repository.GetActiveRoleByIdAsync(request.RoleId);
        if (role is null)
            throw new InvalidOperationException("The selected role is unavailable.");

        if (id == updatedBy && !request.IsActive)
            throw new InvalidOperationException(
                "You cannot deactivate your own account.");

        var userName = request.UserName.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _repository.UserNameOrEmailExistsAsync(userName, email, id))
            throw new InvalidOperationException("Username or email already exists.");

        var isAdmin = user.UserRoleUsers.Any(userRole =>
            userRole.Role.RoleName == "Admin" &&
            userRole.Role.IsActive &&
            !userRole.Role.IsDeleted);
        var assigningAdmin = role.RoleName == "Admin";
        if (assigningAdmin && !isAdmin)
            throw new InvalidOperationException(
                "Admin roles can only be granted through the initial Admin bootstrap process.");

        if (id == updatedBy && isAdmin && !assigningAdmin)
            throw new InvalidOperationException(
                "You cannot remove your own Admin role.");

        if (isAdmin && user.IsActive &&
            (!request.IsActive || !assigningAdmin) &&
            !await _repository.HasOtherActiveAdminAsync(id))
        {
            throw new InvalidOperationException(
                "The last active Admin account cannot be deactivated or demoted.");
        }

        var hasCourierRole = user.UserRoleUsers.Any(userRole =>
            userRole.Role.RoleName == "Courier" &&
            !userRole.Role.IsDeleted);
        var assigningCourier = role.RoleName == "Courier";
        var roleChanged = user.UserRoleUsers.Count != 1 ||
            !user.UserRoleUsers.Any(userRole => userRole.RoleId == role.RoleId);

        if (user.IsActive && !request.IsActive &&
            await _repository.HasActiveCourierAssignmentAsync(id))
        {
            throw new InvalidOperationException(
                "A user with an active Courier assignment cannot be deactivated.");
        }

        if (hasCourierRole && !assigningCourier &&
            await _repository.HasActiveCourierAssignmentAsync(id))
        {
            throw new InvalidOperationException(
                "A Courier role cannot be removed while the Courier has an active parcel assignment.");
        }

        if (roleChanged && assigningCourier)
        {
            var courier = user.CourierUser;
            if (courier is null || courier.IsDeleted)
            {
                throw new InvalidOperationException(
                    "Create this Courier account through Courier registration so its profile and hub are linked.");
            }

            if (courier.Hub is null || !courier.Hub.IsActive || courier.Hub.IsDeleted)
            {
                throw new InvalidOperationException(
                    "The Courier profile must have an active assigned hub before assigning the Courier role.");
            }
        }

        if (roleChanged &&
            role.RoleName == "Customer Viewer" &&
            (user.CustomerUser is null || user.CustomerUser.IsDeleted))
        {
            throw new InvalidOperationException(
                "The Customer Viewer role requires a linked customer profile.");
        }

        var wasActive = user.IsActive;
        user.UserName = userName;
        user.Email = email;
        user.FullName = request.FullName.Trim();
        user.Phone = Normalize(request.Phone);
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = updatedBy;

        await _repository.SetUserRoleAsync(user, role, updatedBy);

        if (user.CourierUser is { IsDeleted: false } courierProfile)
        {
            var updateCourierProfile =
                (hasCourierRole && !assigningCourier) ||
                (roleChanged && assigningCourier) ||
                (hasCourierRole && assigningCourier && wasActive != request.IsActive);

            if (updateCourierProfile)
            {
                courierProfile.IsActive = assigningCourier && request.IsActive;
                if (!courierProfile.IsActive)
                    courierProfile.IsAvailable = false;
                else if (roleChanged)
                    courierProfile.IsAvailable = true;

                courierProfile.UpdatedAt = DateTime.UtcNow;
                courierProfile.UpdatedBy = updatedBy;
            }
        }

        await _repository.SaveChangesAsync();
        return Map(user);
    }

    public async Task<bool> SoftDeleteAsync(int id, int deletedBy)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            return false;

        if (id == deletedBy)
            throw new InvalidOperationException(
                "You cannot delete your own account.");

        var isAdmin = user.UserRoleUsers.Any(userRole =>
            userRole.Role.RoleName == "Admin" &&
            userRole.Role.IsActive &&
            !userRole.Role.IsDeleted);
        if (isAdmin && user.IsActive &&
            !await _repository.HasOtherActiveAdminAsync(id))
        {
            throw new InvalidOperationException(
                "The last active Admin account cannot be deleted.");
        }

        if (await _repository.HasActiveCourierAssignmentAsync(id))
        {
            throw new InvalidOperationException(
                "A user with an active Courier assignment cannot be deleted.");
        }

        user.IsActive = false;
        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = deletedBy;
        if (user.CourierUser is { IsDeleted: false } courierProfile)
        {
            courierProfile.IsActive = false;
            courierProfile.IsAvailable = false;
            courierProfile.UpdatedAt = DateTime.UtcNow;
            courierProfile.UpdatedBy = deletedBy;
        }

        await _repository.SaveChangesAsync();
        return true;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static UserResponseDto Map(User user)
    {
        var activeRoles = user.UserRoleUsers
            .Where(ur => ur.Role.IsActive && !ur.Role.IsDeleted)
            .OrderBy(ur => ur.Role.RoleName)
            .ToList();

        return new UserResponseDto
        {
            UserId = user.UserId,
            RoleId = activeRoles.FirstOrDefault()?.RoleId ?? 0,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Roles = activeRoles.Select(ur => ur.Role.RoleName).Distinct().ToList()
        };
    }
}
