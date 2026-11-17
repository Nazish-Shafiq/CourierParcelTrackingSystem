
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IParcelTrackingRepository
{
    // Get all tracking events with pagination.
    Task<PagedResponseDto<StatusEvent>> GetPagedAsync(
        PaginationRequestDto request,
        int? courierUserId = null);

    // Get tracking history using tracking number.
    Task<List<StatusEvent>> GetTrackingHistoryAsync(
        string trackingNumber,
        int? courierUserId = null);

    // Check whether a parcel is available to the caller, even if it has no events.
    Task<bool> HasAccessibleParcelForTrackingAsync(
        string trackingNumber,
        int? courierUserId = null);

    // Get a parcel for updating its current status.
    Task<Parcel?> GetParcelForUpdateAsync(long parcelId);

    // Get the authenticated user's active Courier profile.
    Task<Courier?> GetActiveCourierForUserAsync(int userId);

    // Get the active assignment for a parcel and Courier.
    Task<ParcelAssignment?> GetActiveAssignmentForCourierAsync(
        long parcelId,
        int courierId);

    // Get an active parcel status.
    Task<ParcelStatus?> GetActiveStatusAsync(int parcelStatusId);

    // Resolve an active parcel status by its code.
    Task<ParcelStatus?> GetActiveStatusByCodeAsync(string statusCode);

    // Get the latest event time for a status code.
    Task<DateTime?> GetLatestStatusEventAtAsync(
        long parcelId,
        string statusCode);

    // Atomically change the parcel status and add its Courier tracking event.
    Task<StatusEvent?> TryApplyCourierStatusTransitionAsync(
        long parcelId,
        int expectedStatusId,
        int targetStatusId,
        string targetStatusCode,
        long parcelAssignmentId,
        int courierId,
        int userId,
        string? notes,
        DateTime eventAt,
        bool requiresDispatcherReassignment);

    // Validate an active hub.
    Task<Hub?> GetActiveHubAsync(int hubId);

    // Validate an active courier.
    Task<Courier?> GetActiveCourierAsync(int courierId);

    // Add a new tracking event.
    Task AddStatusEventAsync(StatusEvent statusEvent);

    // Save database changes.
    Task SaveChangesAsync();
}
