
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelTracking;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;

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
        PaginationRequestDto request)
    {
        var result = await _repository.GetPagedAsync(request);

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
        string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new ArgumentException("Tracking number is required.");

        var events = await _repository.GetTrackingHistoryAsync(
            trackingNumber.Trim());

        if (events.Count == 0)
            throw new KeyNotFoundException(
                "No tracking history found for this tracking number.");

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

        var status = await _repository.GetActiveStatusAsync(
            request.ParcelStatusId);

        if (status == null)
            throw new KeyNotFoundException(
                "Parcel status not found or inactive.");

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
            CourierId = statusEvent.CourierId,
            CourierName = statusEvent.Courier?.FullName,
            EventAt = statusEvent.EventAt,
            Notes = statusEvent.Notes,
            CreatedAt = statusEvent.CreatedAt
        };
    }
}