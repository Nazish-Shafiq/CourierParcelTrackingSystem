using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IParcelRepository
{
    Task<List<Parcel>> GetAllAsync();
    Task<Parcel?> GetByIdAsync(long id);
    Task<(int TotalParcels, int BookedParcels, int DeliveredParcels)> GetCustomerParcelCountsAsync(int userId);
    Task<List<ParcelStatus>> GetCustomerParcelStatusesAsync(int userId);
    Task<PagedResponseDto<Parcel>> GetCustomerParcelsAsync(
        int userId,
        PaginationRequestDto pagination,
        string? search,
        string? statusCode);
    Task<Parcel?> GetCustomerParcelDetailsAsync(int userId, long parcelId);
    Task<Parcel?> GetCustomerParcelByTrackingNumberAsync(
        int userId,
        string trackingNumber);
    Task<bool> CustomerExistsAsync(int customerId);
    Task<bool> RouteExistsAsync(int routeId);
    Task<bool> StatusExistsAsync(int statusId);
    Task<Parcel> CreateAsync(Parcel parcel);
}
