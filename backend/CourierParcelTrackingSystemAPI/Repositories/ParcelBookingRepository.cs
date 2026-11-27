
using CourierParcelTrackingSystem.API.Data;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using RouteEntity = CourierParcelTrackingSystem.API.Entities.Route;

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
        string? search,
        string? filter,
        DateTime? fromBookingDate,
        DateTime? beforeBookingDate)
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
                 p.ParcelRecipient.RecipientName.Contains(search)) ||
                p.CurrentStatus.StatusName.Contains(search) ||
                (p.Route != null && p.Route.RouteName.Contains(search)));
        }

        switch (filter?.Trim().ToLowerInvariant())
        {
            case "today":
            {
                DateTime startOfDay = fromBookingDate ?? DateTime.UtcNow.Date;
                DateTime endOfDay = beforeBookingDate ?? startOfDay.AddDays(1);
                query = query.Where(p =>
                    p.BookingDate >= startOfDay &&
                    p.BookingDate < endOfDay);
                break;
            }
            case "pending":
                query = query.Where(p =>
                    p.CurrentStatus.StatusName.ToLower() == "booked" ||
                    p.CurrentStatus.StatusName.ToLower() == "pending");
                break;
            case "transit":
                query = query.Where(p =>
                    p.CurrentStatus.StatusName.ToLower() == "in transit" ||
                    p.CurrentStatus.StatusName.ToLower() == "out for delivery");
                break;
            case "delivered":
                query = query.Where(p =>
                    p.CurrentStatus.StatusName.ToLower() == "delivered");
                break;
            case "failed":
                query = query.Where(p =>
                    p.CurrentStatus.StatusName.ToLower() == "delivery attempted" ||
                    p.CurrentStatus.StatusName.ToLower().Contains("failed"));
                break;
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
        return await IncludeBookingDetails(_context.Parcels.AsNoTracking())
            .FirstOrDefaultAsync(p => p.ParcelId == id && !p.IsDeleted);
    }

    public async Task<Parcel?> GetByTrackingNumberAsync(string trackingNumber)
    {
        return await IncludeBookingDetails(_context.Parcels.AsNoTracking())
            .FirstOrDefaultAsync(p =>
                p.TrackingNumber == trackingNumber &&
                !p.IsDeleted);
    }

    private static IQueryable<Parcel> IncludeBookingDetails(
        IQueryable<Parcel> query) =>
        query
            .Include(p => p.SenderCustomer)
            .Include(p => p.CurrentStatus)
            .Include(p => p.Route)
            .Include(p => p.ParcelRecipient);

    // Load active related entities so the booking response can be mapped without
    // a second query after the booking transaction commits.
    public Task<Customer?> GetActiveCustomerAsync(int customerId) =>
        _context.Customers.FirstOrDefaultAsync(c =>
            c.CustomerId == customerId &&
            c.IsActive &&
            !c.IsDeleted);

    public Task<RouteEntity?> GetActiveRouteAsync(int routeId) =>
        _context.Routes.FirstOrDefaultAsync(r =>
            r.RouteId == routeId &&
            r.IsActive &&
            !r.IsDeleted);

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
    public async Task<Parcel> AddBookingAsync(
        Parcel parcel,
        ParcelRecipient recipient,
        bool resolveIdempotentConflict)
    {
        parcel.ParcelRecipient = recipient;
        recipient.Parcel = parcel;

        try
        {
            await using (var transaction =
                await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.Parcels.AddAsync(parcel);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return parcel;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        catch (DbUpdateException exception) when (
            resolveIdempotentConflict &&
            IsUniqueConstraintViolation(exception))
        {
            var existing = await GetByTrackingNumberAsync(parcel.TrackingNumber);
            if (existing is not null)
                return existing;

            throw;
        }
    }

    private static bool IsUniqueConstraintViolation(
        DbUpdateException exception)
    {
        for (var inner = exception.InnerException;
            inner is not null;
            inner = inner.InnerException)
        {
            if (inner is SqlException sqlException &&
                sqlException.Errors.Cast<SqlError>().Any(error =>
                    error.Number is 2601 or 2627))
            {
                return true;
            }
        }

        return false;
    }
}
