using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;
using RouteEntity = CourierParcelTrackingSystem.API.Entities.Route;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IParcelBookingRepository
{
    Task<PagedResponseDto<Parcel>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search,
        string? filter,
        DateTime? fromBookingDate,
        DateTime? beforeBookingDate);

    Task<Parcel?> GetByIdAsync(long id);

    Task<Parcel?> GetByTrackingNumberAsync(string trackingNumber);

    Task<Customer?> GetActiveCustomerAsync(int customerId);

    Task<RouteEntity?> GetActiveRouteAsync(int routeId);

    Task<ParcelStatus?> GetBookedStatusAsync();

    Task<bool> TrackingNumberExistsAsync(string trackingNumber);

    Task<Parcel> AddBookingAsync(
        Parcel parcel,
        ParcelRecipient recipient,
        bool resolveIdempotentConflict);
}