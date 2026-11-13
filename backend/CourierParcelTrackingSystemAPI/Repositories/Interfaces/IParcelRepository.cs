using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IParcelRepository
{
    Task<List<Parcel>> GetAllAsync();
    Task<Parcel?> GetByIdAsync(long id);
    Task<bool> CustomerExistsAsync(int customerId);
    Task<bool> RouteExistsAsync(int routeId);
    Task<bool> StatusExistsAsync(int statusId);
    Task<Parcel> CreateAsync(Parcel parcel);
}
