
using CourierParcelTrackingSystem.API.Data;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierParcelTrackingSystem.API.Repositories;

public class ParcelBookingRepository : IParcelBookingRepository
{
    private readonly ApplicationDbContext _context;

    public ParcelBookingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Retrieve paginated parcel bookings.
    public async Task<PagedResponseDto<Parcel>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search)
    {
        int page = Math.Max(1, pagination.Page);
        int pageSize = Math.Clamp(pagination.PageSize, 1, 100);

        IQueryable<Parcel> query = _context.Parcels
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Include(p => p.SenderCustomer)
            .Include(p => p.CurrentStatus)
            .Include(p => p.Route)
            .Include(p => p.ParcelRecipient);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(p =>
                p.TrackingNumber.Contains(search) ||
                p.SenderCustomer.FullName.Contains(search) ||
                (p.ParcelRecipient != null &&
                 p.ParcelRecipient.RecipientName.Contains(search)));
        }

        int totalRecords = await query.CountAsync();

        int totalPages = (int)Math.Ceiling(
            (double)totalRecords / pageSize);

        var parcels = await query
            .OrderByDescending(p => p.BookingDate)
            .ThenByDescending(p => p.ParcelId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponseDto<Parcel>
        {
            Items = parcels,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }

    // Retrieve one parcel booking by ID.
    public async Task<Parcel?> GetByIdAsync(long id)
    {
        return await _context.Parcels
            .AsNoTracking()
            .Where(p => p.ParcelId == id && !p.IsDeleted)
            .Include(p => p.SenderCustomer)
            .Include(p => p.CurrentStatus)
            .Include(p => p.Route)
            .Include(p => p.ParcelRecipient)
            .FirstOrDefaultAsync();
    }

    // Check whether the sender customer exists and is active.
    public async Task<bool> CustomerExistsAsync(int customerId)
    {
        return await _context.Customers.AnyAsync(c =>
            c.CustomerId == customerId &&
            c.IsActive &&
            !c.IsDeleted);
    }

    // Check whether the selected route exists and is active.
    public async Task<bool> RouteExistsAsync(int routeId)
    {
        return await _context.Routes.AnyAsync(r =>
            r.RouteId == routeId &&
            r.IsActive &&
            !r.IsDeleted);
    }

    // Retrieve the initial BOOKED parcel status.
    public async Task<ParcelStatus?> GetBookedStatusAsync()
    {
        return await _context.ParcelStatuses
            .FirstOrDefaultAsync(s =>
                s.StatusCode == "BOOKED" &&
                s.IsActive &&
                !s.IsDeleted);
    }

    // Check tracking number uniqueness.
    public async Task<bool> TrackingNumberExistsAsync(
        string trackingNumber)
    {
        return await _context.Parcels
            .AnyAsync(p => p.TrackingNumber == trackingNumber);
    }

    // Save parcel and recipient together in one transaction.
    public async Task AddBookingAsync(
        Parcel parcel,
        ParcelRecipient recipient)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // Connect recipient with its parcel.
            parcel.ParcelRecipient = recipient;
            recipient.Parcel = parcel;

            await _context.Parcels.AddAsync(parcel);

            // EF Core saves both related records together.
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
