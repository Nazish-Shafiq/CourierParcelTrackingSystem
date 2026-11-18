
using Microsoft.EntityFrameworkCore;
using CourierParcelTrackingSystem.API.Data;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Extensions;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;

namespace CourierParcelTrackingSystem.API.Repositories;

public class HubRepository : IHubRepository
{
    private readonly ApplicationDbContext _context;

    public HubRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Get hubs with pagination and optional search.
    public async Task<PagedResponseDto<Hub>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search,
        bool? activeOnly)
    {
        var query = _context.Hubs
            .AsNoTracking()
            .Where(h => !h.IsDeleted);

        if (activeOnly == true)
            query = query.Where(h => h.IsActive);

        // Apply search when a search value is provided.
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(h =>
                h.HubCode.Contains(search) ||
                h.HubName.Contains(search) ||
                h.AddressLine.Contains(search) ||
                h.City.Contains(search) ||
                h.Country.Contains(search) ||
                (h.Province != null && h.Province.Contains(search)) ||
                (h.PostalCode != null && h.PostalCode.Contains(search))
            );
        }

        // Apply stable ordering before pagination.
        query = query.OrderBy(h => h.HubId);

        return await query.ToPagedResponseAsync(pagination);
    }

    // Get a hub by ID.
    // This returns a tracked entity for update and soft delete operations.
    public async Task<Hub?> GetByIdAsync(int id)
    {
        return await _context.Hubs
            .FirstOrDefaultAsync(h =>
                h.HubId == id && !h.IsDeleted);
    }

    // Check duplicate hub code.
    public async Task<bool> HubCodeExistsAsync(
        string hubCode,
        int? excludeHubId = null)
    {
        var query = _context.Hubs
            .Where(h => h.HubCode == hubCode);

        if (excludeHubId.HasValue)
        {
            query = query.Where(h =>
                h.HubId != excludeHubId.Value);
        }

        return await query.AnyAsync();
    }

    // Add a new hub.
    public async Task AddAsync(Hub hub)
    {
        await _context.Hubs.AddAsync(hub);
        await _context.SaveChangesAsync();
    }

    // Update an existing hub.
    public async Task UpdateAsync(Hub hub)
    {
        await _context.SaveChangesAsync();
    }

    // Soft delete a hub.
    public async Task SoftDeleteAsync(Hub hub)
    {
        hub.IsActive = false;
        hub.IsDeleted = true;

        await _context.SaveChangesAsync();
    }
}
