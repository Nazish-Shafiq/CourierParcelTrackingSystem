using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IParcelBookingRepository
{
    Task<PagedResponseDto<Parcel>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search);

    Task<Parcel?> GetByIdAsync(long id);

    Task<bool> CustomerExistsAsync(int customerId);

    Task<bool> RouteExistsAsync(int routeId);

    Task<ParcelStatus?> GetBookedStatusAsync();

    Task<bool> TrackingNumberExistsAsync(string trackingNumber);

    Task AddBookingAsync(Parcel parcel, ParcelRecipient recipient);
}