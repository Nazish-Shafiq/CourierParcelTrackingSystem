
using CourierParcelTrackingSystem.API.Data;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierParcelTrackingSystem.API.Repositories;

public class ParcelStatusRepository : IParcelStatusRepository
{
    private readonly ApplicationDbContext _context;

    public ParcelStatusRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Get paginated parcel statuses with optional search.
    public async Task<PagedResponseDto<ParcelStatus>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search)
    {
        int page = Math.Max(1, pagination.Page);
        int pageSize = Math.Clamp(pagination.PageSize, 1, 100);

        IQueryable<ParcelStatus> query = _context.ParcelStatuses
            .AsNoTracking()
            .Where(s => !s.IsDeleted);

        // Search by status code, status name, or description.
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(s =>
                s.StatusCode.Contains(search) ||
                s.StatusName.Contains(search) ||
                (s.Description != null &&
                 s.Description.Contains(search)));
        }

        // Calculate total records before pagination.
        int totalRecords = await query.CountAsync();

        int totalPages = (int)Math.Ceiling(
            (double)totalRecords / pageSize);

        // Retrieve requested page.
        var statuses = await query
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.ParcelStatusId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponseDto<ParcelStatus>
        {
            Items = statuses,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }

    // Get parcel status by ID.
    public async Task<ParcelStatus?> GetByIdAsync(int id)
    {
        return await _context.ParcelStatuses
            .FirstOrDefaultAsync(s =>
                s.ParcelStatusId == id &&
                !s.IsDeleted);
    }

    // Check duplicate status name.
    // Deleted records are also checked to avoid unique-index conflicts.
    public async Task<bool> StatusNameExistsAsync(
        string statusName,
        int? excludeStatusId = null)
    {
        return await _context.ParcelStatuses.AnyAsync(s =>
            s.StatusName == statusName &&
            (!excludeStatusId.HasValue ||
             s.ParcelStatusId != excludeStatusId.Value));
    }

    // Check duplicate status code.
    public async Task<bool> StatusCodeExistsAsync(string statusCode)
    {
        return await _context.ParcelStatuses.AnyAsync(s =>
            s.StatusCode == statusCode);
    }

    // Add a new parcel status.
    public async Task AddAsync(ParcelStatus parcelStatus)
    {
        await _context.ParcelStatuses.AddAsync(parcelStatus);

        await _context.SaveChangesAsync();
    }

    // Update an existing tracked parcel status.
    public async Task UpdateAsync(ParcelStatus parcelStatus)
    {
        await _context.SaveChangesAsync();
    }

    // Soft delete parcel status.
    public async Task SoftDeleteAsync(ParcelStatus parcelStatus)
    {
        parcelStatus.IsActive = false;
        parcelStatus.IsDeleted = true;

        await _context.SaveChangesAsync();
    }
}
