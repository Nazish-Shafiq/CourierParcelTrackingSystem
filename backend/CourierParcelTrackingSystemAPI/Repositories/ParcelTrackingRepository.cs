
using Microsoft.EntityFrameworkCore;

using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Data; // Adjust if your ApplicationDbContext uses another namespace.

namespace CourierParcelTrackingSystem.API.Repositories.Implementations;

public class ParcelTrackingRepository : IParcelTrackingRepository
{
    private readonly ApplicationDbContext _context;

    public ParcelTrackingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Get all tracking events with pagination.
    public async Task<PagedResponseDto<StatusEvent>> GetPagedAsync(
        PaginationRequestDto request)
    {
        var query = _context.StatusEvents
            .AsNoTracking()
            .Include(e => e.Parcel)
            .Include(e => e.ParcelStatus)
            .Include(e => e.Hub)
            .Include(e => e.Courier)
            .Where(e => e.Parcel.IsActive && !e.Parcel.IsDeleted)
            .OrderByDescending(e => e.EventAt);

        var totalRecords = await query.CountAsync();

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResponseDto<StatusEvent>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(
                (double)totalRecords / request.PageSize)
        };
    }

    // Get complete tracking history using tracking number.
    public async Task<List<StatusEvent>> GetTrackingHistoryAsync(
        string trackingNumber)
    {
        return await _context.StatusEvents
            .AsNoTracking()
            .Include(e => e.Parcel)
            .Include(e => e.ParcelStatus)
            .Include(e => e.Hub)
            .Include(e => e.Courier)
            .Where(e =>
                e.Parcel.TrackingNumber == trackingNumber &&
                e.Parcel.IsActive &&
                !e.Parcel.IsDeleted)
            .OrderBy(e => e.EventAt)
            .ToListAsync();
    }

    // Get a parcel as a tracked entity for status updates.
    public async Task<Parcel?> GetParcelForUpdateAsync(long parcelId)
    {
        return await _context.Parcels
            .FirstOrDefaultAsync(p =>
                p.ParcelId == parcelId &&
                p.IsActive &&
                !p.IsDeleted);
    }

    // Get an active parcel status.
    public async Task<ParcelStatus?> GetActiveStatusAsync(
        int parcelStatusId)
    {
        return await _context.ParcelStatuses
            .FirstOrDefaultAsync(s =>
                s.ParcelStatusId == parcelStatusId &&
                s.IsActive &&
                !s.IsDeleted);
    }

    // Get an active hub.
    public async Task<Hub?> GetActiveHubAsync(int hubId)
    {
        return await _context.Hubs
            .FirstOrDefaultAsync(h =>
                h.HubId == hubId &&
                h.IsActive &&
                !h.IsDeleted);
    }

    // Get an active courier.
    public async Task<Courier?> GetActiveCourierAsync(int courierId)
    {
        return await _context.Couriers
            .FirstOrDefaultAsync(c =>
                c.CourierId == courierId &&
                c.IsActive &&
                !c.IsDeleted);
    }

    // Add a new tracking event.
    public async Task AddStatusEventAsync(StatusEvent statusEvent)
    {
        await _context.StatusEvents.AddAsync(statusEvent);
    }

    // Save database changes.
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}