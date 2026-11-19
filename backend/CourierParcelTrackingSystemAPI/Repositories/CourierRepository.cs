
using Microsoft.EntityFrameworkCore;
using CourierParcelTrackingSystem.API.Data;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Extensions;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;

namespace CourierParcelTrackingSystem.API.Repositories;

public class CourierRepository : ICourierRepository
{
    private readonly ApplicationDbContext _context;

    public CourierRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Get couriers with pagination and optional search.
    public async Task<PagedResponseDto<Courier>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search,
        bool? isAvailable,
        bool? isActive)
    {
        var query = _context.Couriers
            .Include(c => c.Hub)
            .Include(c => c.User)
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        if (isAvailable.HasValue)
            query = query.Where(c => c.IsAvailable == isAvailable.Value);
        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        // Apply search when a search value is provided.
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(c =>
                c.EmployeeCode.Contains(search) ||
                c.FullName.Contains(search) ||
                c.Phone.Contains(search) ||
                (c.Email != null && c.Email.Contains(search)) ||
                (c.VehicleType != null && c.VehicleType.Contains(search)) ||
                (c.VehicleNumber != null && c.VehicleNumber.Contains(search))
            );
        }

        // Apply stable ordering before pagination.
        query = query.OrderBy(c => c.CourierId);

        return await query.ToPagedResponseAsync(pagination);
    }

    public Task<List<Courier>> GetActiveCouriersForDispatchAsync(int hubId) =>
        _context.Couriers
            .Include(c => c.Hub)
            .AsNoTracking()
            .Where(c =>
                c.IsActive &&
                !c.IsDeleted &&
                c.IsAvailable &&
                c.HubId == hubId &&
                c.Hub != null &&
                c.Hub.IsActive &&
                !c.Hub.IsDeleted &&
                c.UserId.HasValue &&
                c.User != null &&
                c.User.IsActive &&
                !c.User.IsDeleted &&
                c.User.UserRoleUsers.Any(userRole =>
                    userRole.Role.RoleName == "Courier" &&
                    userRole.Role.IsActive &&
                    !userRole.Role.IsDeleted) &&
                !c.ParcelAssignments.Any(a => a.UnassignedAt == null))
            .OrderBy(c => c.CourierId)
            .ToListAsync();

    // Get courier by ID.
    // Returns a tracked entity for update and soft delete.
    public async Task<Courier?> GetByIdAsync(int id)
    {
        return await _context.Couriers
            .Include(c => c.Hub)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c =>
                c.CourierId == id && !c.IsDeleted);
    }

    // Check duplicate employee code.
    public async Task<bool> EmployeeCodeExistsAsync(
        string employeeCode,
        int? excludeCourierId = null)
    {
        var query = _context.Couriers
            .Where(c => c.EmployeeCode == employeeCode);

        if (excludeCourierId.HasValue)
        {
            query = query.Where(c =>
                c.CourierId != excludeCourierId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<string> GenerateUniqueEmployeeCodeAsync()
    {
        string employeeCode;
        do
        {
            employeeCode = $"COU-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
        }
        while (await EmployeeCodeExistsAsync(employeeCode));

        return employeeCode;
    }

    public Task<bool> IsActiveCourierUserAsync(int userId) =>
        _context.Users.AnyAsync(user =>
            user.UserId == userId &&
            user.IsActive && !user.IsDeleted &&
            user.UserRoleUsers.Any(userRole =>
                userRole.Role.RoleName == "Courier" &&
                userRole.Role.IsActive && !userRole.Role.IsDeleted));

    public Task<bool> UserHasCourierProfileAsync(int userId, int? excludeCourierId = null)
    {
        var query = _context.Couriers.Where(courier => courier.UserId == userId);
        if (excludeCourierId.HasValue)
            query = query.Where(courier => courier.CourierId != excludeCourierId.Value);

        return query.AnyAsync();
    }

    public Task<bool> HasActiveAssignmentAsync(int courierId) =>
        _context.ParcelAssignments.AnyAsync(assignment =>
            assignment.CourierId == courierId &&
            assignment.UnassignedAt == null);

    // Update an existing courier.
    public async Task UpdateAsync(Courier courier)
    {
        await _context.SaveChangesAsync();
    }

    // Soft delete a courier.
    public async Task SoftDeleteAsync(Courier courier)
    {
        courier.IsActive = false;
        courier.IsDeleted = true;

        await _context.SaveChangesAsync();
    }
}
