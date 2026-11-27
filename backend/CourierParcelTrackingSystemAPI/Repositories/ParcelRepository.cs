using CourierParcelTrackingSystem.API.Data;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using RouteEntity = CourierParcelTrackingSystem.API.Entities.Route;

namespace CourierParcelTrackingSystem.API.Repositories;

public class ParcelRepository : IParcelRepository
{
    private readonly ApplicationDbContext _context;
    public ParcelRepository(ApplicationDbContext context) => _context = context;

    private IQueryable<Parcel> ParcelQuery() =>
        _context.Parcels.AsNoTracking()
            .Where(p => p.IsActive && !p.IsDeleted)
            .Include(p => p.SenderCustomer)
            .Include(p => p.CurrentStatus)
            .Include(p => p.CurrentHub)
            .Include(p => p.Route)
                .ThenInclude(r => r!.OriginHub)
            .Include(p => p.Route)
                .ThenInclude(r => r!.DestinationHub);

    public Task<List<Parcel>> GetAllAsync() =>
        ParcelQuery().OrderByDescending(p => p.BookingDate).ToListAsync();

    public Task<Parcel?> GetByIdAsync(long id) =>
        ParcelQuery()
            .Include(p => p.StatusEvents
                .Where(statusEvent =>
                    statusEvent.ParcelStatus.StatusCode == "DELIVERED"))
            .FirstOrDefaultAsync(p => p.ParcelId == id);

    public async Task<(int TotalParcels, int BookedParcels, int DeliveredParcels)> GetCustomerParcelCountsAsync(
        int userId)
    {
        var query = CustomerParcelQuery(userId);
        var total = await query.CountAsync();
        var booked = await query.CountAsync(p => p.CurrentStatus.StatusCode == "BOOKED");
        var delivered = await query.CountAsync(p => p.CurrentStatus.StatusCode == "DELIVERED");

        return (total, booked, delivered);
    }

    public Task<List<ParcelStatus>> GetCustomerParcelStatusesAsync(int userId) =>
        CustomerParcelQuery(userId)
            .Select(parcel => parcel.CurrentStatus)
            .Distinct()
            .OrderBy(status => status.StatusName)
            .ToListAsync();

    public async Task<PagedResponseDto<Parcel>> GetCustomerParcelsAsync(
        int userId,
        PaginationRequestDto pagination,
        string? search,
        string? statusCode)
    {
        IQueryable<Parcel> query = CustomerParcelQuery(userId)
            .Include(p => p.SenderCustomer)
            .Include(p => p.CurrentStatus)
            .Include(p => p.ParcelRecipient)
            .Include(p => p.Route)
                .ThenInclude(r => r!.OriginHub)
            .Include(p => p.Route)
                .ThenInclude(r => r!.DestinationHub);

        if (!string.IsNullOrWhiteSpace(statusCode) &&
            !string.Equals(statusCode, "ALL", StringComparison.OrdinalIgnoreCase))
        {
            var requestedStatus = statusCode.Trim();
            query = query.Where(p => p.CurrentStatus.StatusCode == requestedStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var requestedSearch = search.Trim();
            query = query.Where(p =>
                p.TrackingNumber.Contains(requestedSearch) ||
                p.CurrentStatus.StatusName.Contains(requestedSearch) ||
                (p.Route != null &&
                    (p.Route.OriginHub.HubName.Contains(requestedSearch) ||
                     p.Route.DestinationHub.HubName.Contains(requestedSearch))));
        }

        var totalRecords = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.BookingDate)
            .ThenByDescending(p => p.ParcelId)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedResponseDto<Parcel>
        {
            Items = items,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize)
        };
    }

    public Task<Parcel?> GetCustomerParcelDetailsAsync(int userId, long parcelId) =>
        CustomerParcelDetailsQuery(userId)
            .FirstOrDefaultAsync(p => p.ParcelId == parcelId);

    public Task<Parcel?> GetCustomerParcelByTrackingNumberAsync(
        int userId,
        string trackingNumber) =>
        CustomerParcelDetailsQuery(userId)
            .FirstOrDefaultAsync(p => p.TrackingNumber == trackingNumber);

    private IQueryable<Parcel> CustomerParcelDetailsQuery(int userId) =>
        CustomerParcelQuery(userId)
            .Include(p => p.SenderCustomer)
            .Include(p => p.CurrentStatus)
            .Include(p => p.ParcelRecipient)
            .Include(p => p.CurrentHub)
            .Include(p => p.Route)
                .ThenInclude(r => r!.OriginHub)
            .Include(p => p.Route)
                .ThenInclude(r => r!.DestinationHub)
            .Include(p => p.StatusEvents)
                .ThenInclude(e => e.ParcelStatus)
            .Include(p => p.StatusEvents)
                .ThenInclude(e => e.Courier)
            .Include(p => p.StatusEvents)
                .ThenInclude(e => e.Hub);

    private IQueryable<Parcel> CustomerParcelQuery(int userId) =>
        _context.Parcels
            .AsNoTracking()
            .Where(p =>
                p.IsActive &&
                !p.IsDeleted &&
                p.CurrentStatus.IsActive &&
                !p.CurrentStatus.IsDeleted &&
                p.SenderCustomer.IsActive &&
                !p.SenderCustomer.IsDeleted &&
                p.SenderCustomer.UserId == userId &&
                p.SenderCustomer.User != null &&
                p.SenderCustomer.User.IsActive &&
                !p.SenderCustomer.User.IsDeleted);

    public Task<bool> CustomerExistsAsync(int customerId) =>
        _context.Customers.AnyAsync(c => c.CustomerId == customerId && c.IsActive && !c.IsDeleted);

    public Task<bool> RouteExistsAsync(int routeId) =>
        _context.Set<RouteEntity>().AnyAsync(r => r.RouteId == routeId && r.IsActive && !r.IsDeleted);

    public Task<bool> StatusExistsAsync(int statusId) =>
        _context.ParcelStatuses.AnyAsync(s => s.ParcelStatusId == statusId && s.IsActive && !s.IsDeleted);

    public async Task<Parcel> CreateAsync(Parcel parcel)
    {
        _context.Parcels.Add(parcel);
        await _context.SaveChangesAsync();
        return await ParcelQuery().FirstAsync(p => p.ParcelId == parcel.ParcelId);
    }
}
