
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using RouteEntity = CourierParcelTrackingSystem.API.Entities.Route;

// Change this using only if your ApplicationDbContext
// is in a different namespace.
using CourierParcelTrackingSystem.API.Data;

namespace CourierParcelTrackingSystem.API.Repositories;

public class RouteRepository : IRouteRepository
{
    private readonly ApplicationDbContext _context;

    public RouteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Get paginated routes with optional search.
    public async Task<PagedResponseDto<RouteEntity>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search,
        bool? isActive)
    {
        int page = Math.Max(1, pagination.Page);
        int pageSize = Math.Clamp(pagination.PageSize, 1, 100);

        IQueryable<RouteEntity> query = _context.Routes
            .AsNoTracking()
            .Include(r => r.OriginHub)
            .Include(r => r.DestinationHub)
            .Where(r => !r.IsDeleted);

        if (isActive.HasValue)
            query = query.Where(r => r.IsActive == isActive.Value);

        // Search by route code, route name, or hub names.
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(r =>
                r.RouteCode.Contains(search) ||
                r.RouteName.Contains(search) ||
                r.OriginHub.HubName.Contains(search) ||
                r.DestinationHub.HubName.Contains(search));
        }

        int totalRecords = await query.CountAsync();

        int totalPages = (int)Math.Ceiling(
            (double)totalRecords / pageSize);

        var routes = await query
            .OrderBy(r => r.RouteId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponseDto<RouteEntity>
        {
            Items = routes,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }

    // Get route by ID, including origin and destination hubs.
    public async Task<RouteEntity?> GetByIdAsync(int id)
    {
        return await _context.Routes
            .Include(r => r.OriginHub)
            .Include(r => r.DestinationHub)
            .FirstOrDefaultAsync(r =>
                r.RouteId == id &&
                !r.IsDeleted);
    }

    // Check whether the hub exists, is active, and is not deleted.
    public async Task<bool> HubExistsAsync(int hubId)
    {
        return await _context.Hubs.AnyAsync(h =>
            h.HubId == hubId &&
            h.IsActive &&
            !h.IsDeleted);
    }

    // Create route.
    public async Task AddAsync(RouteEntity route)
    {
        await _context.Routes.AddAsync(route);
        await _context.SaveChangesAsync();
    }

    // Update tracked route.
    public async Task UpdateAsync(RouteEntity route)
    {
        await _context.SaveChangesAsync();
    }

    // Soft delete route.
    public async Task SoftDeleteAsync(RouteEntity route)
    {
        route.IsActive = false;
        route.IsDeleted = true;

        await _context.SaveChangesAsync();
    }
}
