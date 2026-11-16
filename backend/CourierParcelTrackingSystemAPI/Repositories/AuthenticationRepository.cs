using CourierParcelTrackingSystem.API.Data;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace CourierParcelTrackingSystem.API.Repositories;
public class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _context;
    public AuthRepository(ApplicationDbContext context) => _context = context;

    public Task<User?> GetUserForLoginAsync(string usernameOrEmail) =>
        _context.Users.FirstOrDefaultAsync(u =>
            (u.UserName == usernameOrEmail || u.Email == usernameOrEmail) &&
            u.IsActive && !u.IsDeleted);

    public Task<User?> GetUserWithRolesAsync(int userId) =>
        _context.Users.Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive && !u.IsDeleted);

    public Task<Role?> GetActiveRoleByIdAsync(int roleId) =>
        _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId && r.IsActive && !r.IsDeleted);

    public Task<Role?> GetActiveRoleByNameAsync(string roleName) =>
        _context.Roles.FirstOrDefaultAsync(r =>
            r.RoleName == roleName && r.IsActive && !r.IsDeleted);

    public Task<List<Role>> GetActiveRolesAsync() =>
        _context.Roles
            .Where(r => r.IsActive && !r.IsDeleted)
            .OrderBy(r => r.RoleName)
            .ToListAsync();

    // Include deleted users because unique database indexes still reserve their usernames/emails.
    public Task<bool> UserExistsAsync(string username, string email) =>
        _context.Users.AnyAsync(u => u.UserName == username || u.Email == email);

    public Task<bool> AdminExistsAsync() =>
        _context.UserRoles.AnyAsync(ur =>
            ur.Role.RoleName == "Admin" &&
            ur.Role.IsActive && !ur.Role.IsDeleted &&
            ur.User.IsActive && !ur.User.IsDeleted);

    public async Task CreateAccountAsync(
        User user,
        Role role,
        int? createdBy,
        Customer? customer = null,
        Courier? courier = null)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            user.CreatedBy = createdBy;
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;
            user.IsDeleted = false;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.UserRoles.Add(new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                AssignedBy = createdBy,
                AssignedAt = DateTime.UtcNow
            });

            if (customer != null)
            {
                customer.UserId = user.UserId;
                customer.CreatedBy = createdBy;
                customer.CreatedAt = DateTime.UtcNow;
                customer.IsActive = true;
                customer.IsDeleted = false;
                _context.Customers.Add(customer);
            }

            if (courier != null)
            {
                if (!string.Equals(role.RoleName, "Courier", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("A Courier profile can only be created for a Courier role.");

                if (await _context.Couriers.AnyAsync(c => c.UserId == user.UserId))
                    throw new InvalidOperationException("A Courier profile is already linked to this user.");

                courier.UserId = user.UserId;
                courier.CreatedBy = createdBy;
                courier.CreatedAt = DateTime.UtcNow;
                courier.IsAvailable = true;
                courier.IsActive = true;
                courier.IsDeleted = false;
                _context.Couriers.Add(courier);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdatePasswordHashAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}
