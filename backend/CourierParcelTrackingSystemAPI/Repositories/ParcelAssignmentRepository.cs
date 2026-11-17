
using Microsoft.EntityFrameworkCore;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Data; // Change this if your DbContext has a different namespace.

namespace CourierParcelTrackingSystem.API.Repositories.Implementations;

public class ParcelAssignmentRepository : IParcelAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public ParcelAssignmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Get all assignment records with pagination.
    public async Task<PagedResponseDto<ParcelAssignment>> GetPagedAsync(
        PaginationRequestDto request)
    {
        var query = _context.ParcelAssignments
            .AsNoTracking()
            .Include(a => a.Parcel)
            .Include(a => a.Courier)
            .OrderByDescending(a => a.AssignedAt);

        var totalRecords = await query.CountAsync();

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResponseDto<ParcelAssignment>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(
                (double)totalRecords / request.PageSize)
        };
    }

    // Get assignment details by ID.
    public async Task<ParcelAssignment?> GetByIdAsync(long assignmentId)
    {
        return await _context.ParcelAssignments
            .Include(a => a.Parcel)
            .ThenInclude(p => p.CurrentStatus)
            .Include(a => a.Courier)
            .Include(a => a.AssignedByNavigation)
            .Include(a => a.UnassignedByNavigation)
            .FirstOrDefaultAsync(a =>
                a.ParcelAssignmentId == assignmentId);
    }

    // Get assignments that are currently active.
    public async Task<List<ParcelAssignment>> GetActiveAssignmentsAsync()
    {
        return await _context.ParcelAssignments
            .AsNoTracking()
            .Include(a => a.Parcel)
            .Include(a => a.Courier)
            .Where(a => a.UnassignedAt == null)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();
    }

    // Get an active parcel for assignment.
    public async Task<Parcel?> GetParcelForAssignmentAsync(long parcelId)
    {
        return await _context.Parcels
            .Include(p => p.CurrentStatus)
            .FirstOrDefaultAsync(p =>
                p.ParcelId == parcelId &&
                p.IsActive &&
                !p.IsDeleted);
    }

    // Get an active courier for assignment.
    public async Task<Courier?> GetCourierForAssignmentAsync(int courierId)
    {
        return await _context.Couriers
            .FirstOrDefaultAsync(c =>
                c.CourierId == courierId &&
                c.IsActive &&
                !c.IsDeleted &&
                c.UserId.HasValue &&
                c.User != null &&
                c.User.IsActive &&
                !c.User.IsDeleted &&
                c.User.UserRoleUsers.Any(userRole =>
                    userRole.Role.RoleName == "Courier" &&
                    userRole.Role.IsActive &&
                    !userRole.Role.IsDeleted));
    }

    // Check whether the parcel already has an active assignment.
    public async Task<bool> HasActiveAssignmentAsync(long parcelId)
    {
        return await _context.ParcelAssignments
            .AnyAsync(a =>
                a.ParcelId == parcelId &&
                a.UnassignedAt == null);
    }

    // Check whether the courier already has an active assignment.
    public async Task<bool> HasActiveAssignmentForCourierAsync(int courierId)
    {
        return await _context.ParcelAssignments
            .AnyAsync(a =>
                a.CourierId == courierId &&
                a.UnassignedAt == null);
    }

    public async Task<DateTime?> GetLatestDeliveryAttemptAtAsync(long parcelId)
    {
        return await _context.StatusEvents
            .Where(e =>
                e.ParcelId == parcelId &&
                e.ParcelStatus.StatusCode == "DELIVERY_ATTEMPTED")
            .OrderByDescending(e => e.EventAt)
            .ThenByDescending(e => e.StatusEventId)
            .Select(e => (DateTime?)e.EventAt)
            .FirstOrDefaultAsync();
    }

    // Add a new assignment record.
    public async Task AddAsync(ParcelAssignment assignment)
    {
        await _context.ParcelAssignments.AddAsync(assignment);
    }

    // Save all pending changes.
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
