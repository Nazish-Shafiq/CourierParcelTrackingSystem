using CourierParcelTrackingSystem.API.Data;
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
            .Include(p => p.Route);

    public Task<List<Parcel>> GetAllAsync() =>
        ParcelQuery().OrderByDescending(p => p.BookingDate).ToListAsync();

    public Task<Parcel?> GetByIdAsync(long id) =>
        ParcelQuery().FirstOrDefaultAsync(p => p.ParcelId == id);

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
