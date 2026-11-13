
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IParcelTrackingRepository
{
    // Get all tracking events with pagination.
    Task<PagedResponseDto<StatusEvent>> GetPagedAsync(
        PaginationRequestDto request);

    // Get tracking history using tracking number.
    Task<List<StatusEvent>> GetTrackingHistoryAsync(
        string trackingNumber);

    // Get a parcel for updating its current status.
    Task<Parcel?> GetParcelForUpdateAsync(long parcelId);

    // Get an active parcel status.
    Task<ParcelStatus?> GetActiveStatusAsync(int parcelStatusId);

    // Validate an active hub.
    Task<Hub?> GetActiveHubAsync(int hubId);

    // Validate an active courier.
    Task<Courier?> GetActiveCourierAsync(int courierId);

    // Add a new tracking event.
    Task AddStatusEventAsync(StatusEvent statusEvent);

    // Save database changes.
    Task SaveChangesAsync();
}
