
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelTracking;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using RouteEntity = CourierParcelTrackingSystem.API.Entities.Route;

namespace CourierParcelTrackingSystem.API.Services.Implementations;

public class ParcelTrackingService : IParcelTrackingService
{
    private readonly IParcelTrackingRepository _repository;

    public ParcelTrackingService(IParcelTrackingRepository repository)
    {
        _repository = repository;
    }

    // Get tracking events with pagination.
    public async Task<PagedResponseDto<ParcelTrackingResponseDto>> GetPagedAsync(
        PaginationRequestDto request,
        int? courierUserId = null)
    {
        var result = await _repository.GetPagedAsync(request, courierUserId);

        return new PagedResponseDto<ParcelTrackingResponseDto>
        {
            Items = result.Items.Select(MapToResponse).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages
        };
    }

    // Get complete tracking history using tracking number.
    public async Task<List<ParcelTrackingResponseDto>> GetTrackingHistoryAsync(
        string trackingNumber,
        int? courierUserId = null)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new ArgumentException("Tracking number is required.");

        var events = await _repository.GetTrackingHistoryAsync(
            trackingNumber.Trim(),
            courierUserId);

        if (events.Count == 0 &&
            !await _repository.HasAccessibleParcelForTrackingAsync(
                trackingNumber.Trim(),
                courierUserId))
        {
            throw new KeyNotFoundException(
                "No tracking history found for this tracking number.");
        }

        return events.Select(MapToResponse).ToList();
    }

    // Create a tracking event and update current parcel status.
    public async Task<ParcelTrackingResponseDto> CreateStatusEventAsync(
        CreateStatusEventRequestDto request,
        int? createdBy)
    {
        var parcel = await _repository.GetParcelForUpdateAsync(
            request.ParcelId);

        if (parcel == null)
            throw new KeyNotFoundException("Parcel not found or inactive.");

        if (!parcel.CurrentStatus.IsActive || parcel.CurrentStatus.IsDeleted)
        {
            throw new KeyNotFoundException(
                "The parcel's current status is not active.");
        }

        if (parcel.DeliveredAt.HasValue ||
            parcel.CurrentStatus.IsTerminal ||
            parcel.CurrentStatus.StatusCode.Equals(
                "DELIVERED",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "A delivered or terminal parcel cannot be updated.");
        }

        var status = await _repository.GetActiveStatusAsync(
            request.ParcelStatusId);

        if (status == null)
            throw new KeyNotFoundException(
                "Parcel status not found or inactive.");

        var currentCode = parcel.CurrentStatus.StatusCode;
        if (!ParcelStatusTransitionRules.IsGeneralAllowed(
            currentCode,
            status.StatusCode))
        {
            if (currentCode.Trim().Equals(
                "ON_HOLD",
                StringComparison.OrdinalIgnoreCase))
            {
                var resumeCode = await _repository
                    .GetStatusCodeBeforeOnHoldAsync(parcel.ParcelId);
                if (!ParcelStatusTransitionRules.IsGeneralAllowed(
                    currentCode,
                    status.StatusCode,
                    resumeCode))
                {
                    throw new InvalidOperationException(
                        $"Parcel transition from {currentCode} to {status.StatusCode} is not allowed.");
                }
            }
            else
            {
                throw new InvalidOperationException(
                    $"Parcel transition from {currentCode} to {status.StatusCode} is not allowed.");
            }
        }

        // Validate hub when supplied.
        Hub? hub = null;

        if (request.HubId.HasValue)
        {
            hub = await _repository.GetActiveHubAsync(request.HubId.Value);

            if (hub == null)
                throw new KeyNotFoundException("Hub not found or inactive.");
        }

        // Validate courier when supplied.
        Courier? courier = null;

        if (request.CourierId.HasValue)
        {
            courier = await _repository.GetActiveCourierAsync(
                request.CourierId.Value);

            if (courier == null)
                throw new KeyNotFoundException(
                    "Courier not found or inactive.");
        }

        var now = DateTime.UtcNow;

        var statusEvent = new StatusEvent
        {
            ParcelId = parcel.ParcelId,
            ParcelStatusId = status.ParcelStatusId,
            HubId = hub?.HubId,
            CourierId = courier?.CourierId,
            EventAt = now,
            Notes = request.Notes,
            CreatedAt = now,
            CreatedBy = createdBy
        };

        // Keep the parcel's current status synchronized.
        parcel.CurrentStatusId = status.ParcelStatusId;
        parcel.UpdatedAt = now;
        parcel.UpdatedBy = createdBy;

        // Update delivery timestamp when parcel is delivered.
        if (status.StatusCode.Equals(
            "DELIVERED",
            StringComparison.OrdinalIgnoreCase))
        {
            parcel.DeliveredAt = now;
        }

        await _repository.AddStatusEventAsync(statusEvent);
        await _repository.SaveChangesAsync();

        // Load navigation properties for response mapping.
        var trackingHistory = await _repository.GetTrackingHistoryAsync(
            parcel.TrackingNumber);

        var savedEvent = trackingHistory
            .OrderByDescending(e => e.EventAt)
            .ThenByDescending(e => e.StatusEventId)
            .First();

        return MapToResponse(savedEvent);
    }

    // Update parcel status by parcel ID.
    public async Task<ParcelTrackingResponseDto> UpdateParcelStatusAsync(
        long parcelId,
        UpdateParcelStatusRequestDto request,
        int? createdBy)
    {
        var createRequest = new CreateStatusEventRequestDto
        {
            ParcelId = parcelId,
            ParcelStatusId = request.ParcelStatusId,
            HubId = request.HubId,
            CourierId = request.CourierId,
            Notes = request.Notes
        };

        return await CreateStatusEventAsync(createRequest, createdBy);
    }

    public async Task<ParcelTrackingResponseDto> UpdateCourierParcelStatusAsync(
        long parcelId,
        UpdateCourierParcelStatusRequestDto request,
        int userId)
    {
        var courier = await _repository.GetActiveCourierForUserAsync(userId);
        if (courier == null)
        {
            throw new KeyNotFoundException(
                "No active Courier profile was found for the logged-in user.");
        }

        var parcel = await _repository.GetParcelForUpdateAsync(parcelId);
        if (parcel == null)
        {
            throw new KeyNotFoundException("Parcel not found or inactive.");
        }

        if (!parcel.CurrentStatus.IsActive || parcel.CurrentStatus.IsDeleted)
        {
            throw new KeyNotFoundException(
                "The parcel's current status is not active.");
        }

        var assignment = await _repository.GetActiveAssignmentForCourierAsync(
            parcelId,
            courier.CourierId);
        if (assignment == null)
        {
            throw new UnauthorizedAccessException(
                "This parcel is not actively assigned to your Courier profile.");
        }

        if (parcel.DeliveredAt.HasValue ||
            parcel.CurrentStatus.IsTerminal ||
            parcel.CurrentStatus.StatusCode.Equals(
                "DELIVERED",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "A delivered or terminal parcel cannot be updated.");
        }

        if (string.IsNullOrWhiteSpace(request.StatusCode))
        {
            throw new ArgumentException("A Courier status code is required.");
        }
        var targetStatusCode = request.StatusCode.Trim().ToUpperInvariant();

        if (targetStatusCode is not (
            "PICKED_UP" or
            "IN_TRANSIT" or
            "AT_DESTINATION_HUB" or
            "OUT_FOR_DELIVERY" or
            "DELIVERED" or
            "DELIVERY_ATTEMPTED"))
        {
            throw new ArgumentException(
                "This status is not available as a Courier action.");
        }

        var targetStatus = await _repository.GetActiveStatusByCodeAsync(
            targetStatusCode);
        if (targetStatus == null)
        {
            throw new KeyNotFoundException(
                $"The active {targetStatusCode} parcel status was not found.");
        }

        var currentStatusCode = parcel.CurrentStatus.StatusCode;
        var requiresDispatcherReassignment = false;
        var isAllowed = ParcelStatusTransitionRules.IsCourierAllowed(
            currentStatusCode,
            targetStatusCode);

        if (currentStatusCode.Equals(
            "DELIVERY_ATTEMPTED",
            StringComparison.OrdinalIgnoreCase) &&
            targetStatusCode == "OUT_FOR_DELIVERY")
        {
            requiresDispatcherReassignment = true;
            var latestAttemptAt = await _repository.GetLatestStatusEventAtAsync(
                parcelId,
                "DELIVERY_ATTEMPTED");

            if (!latestAttemptAt.HasValue ||
                assignment.AssignedAt <= latestAttemptAt.Value)
            {
                throw new InvalidOperationException(
                    "A Dispatcher must assign or reassign this parcel after the failed delivery before another attempt.");
            }
        }

        if (!isAllowed)
        {
            throw new InvalidOperationException(
                $"Courier transition from {currentStatusCode} to {targetStatusCode} is not allowed.");
        }

        var statusEvent = await _repository.TryApplyCourierStatusTransitionAsync(
            parcelId,
            parcel.CurrentStatusId,
            targetStatus.ParcelStatusId,
            targetStatusCode,
            assignment.ParcelAssignmentId,
            courier.CourierId,
            userId,
            request.Notes,
            DateTime.UtcNow,
            requiresDispatcherReassignment);

        if (statusEvent == null)
        {
            throw new InvalidOperationException(
                "The parcel status or assignment changed. Refresh the parcel and try again.");
        }

        return MapToResponse(statusEvent);
    }

    // Map StatusEvent entity to response DTO.
    private static ParcelTrackingResponseDto MapToResponse(
        StatusEvent statusEvent)
    {
        return new ParcelTrackingResponseDto
        {
            StatusEventId = statusEvent.StatusEventId,
            ParcelId = statusEvent.ParcelId,
            TrackingNumber = statusEvent.Parcel?.TrackingNumber
                ?? string.Empty,
            ParcelStatusId = statusEvent.ParcelStatusId,
            StatusName = statusEvent.ParcelStatus?.StatusName
                ?? string.Empty,
            StatusCode = statusEvent.ParcelStatus?.StatusCode
                ?? string.Empty,
            HubId = statusEvent.HubId,
            HubName = statusEvent.Hub?.HubName,
            OriginHubName = RouteHubName(
                statusEvent.Parcel?.Route,
                statusEvent.Parcel?.Route?.OriginHub),
            DestinationHubName = RouteHubName(
                statusEvent.Parcel?.Route,
                statusEvent.Parcel?.Route?.DestinationHub),
            CourierId = statusEvent.CourierId,
            CourierName = statusEvent.Courier?.FullName,
            EventAt = statusEvent.EventAt,
            Notes = statusEvent.Notes,
            CreatedAt = statusEvent.CreatedAt
        };
    }

    private static string? RouteHubName(RouteEntity? route, Hub? hub) =>
        route is not null ? hub?.HubName : null;
}