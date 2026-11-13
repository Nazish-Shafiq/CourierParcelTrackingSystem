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
            .Where(u => u.IsActive && !u.IsDeleted)
            .Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role)
            .OrderBy(u => u.UserId).ToListAsync();

    public Task<User?> GetByIdAsync(int id) =>
        _context.Users.AsNoTracking()
            .Where(u => u.UserId == id && u.IsActive && !u.IsDeleted)
            .Include(u => u.UserRoleUsers).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync();
}
