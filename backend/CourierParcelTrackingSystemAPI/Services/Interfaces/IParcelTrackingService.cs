
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelTracking;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface IParcelTrackingService
{
    // Get tracking events with pagination.
    Task<PagedResponseDto<ParcelTrackingResponseDto>> GetPagedAsync(
        PaginationRequestDto request,
        int? courierUserId = null);

    // Get complete tracking history by tracking number.
    Task<List<ParcelTrackingResponseDto>> GetTrackingHistoryAsync(
        string trackingNumber,
        int? courierUserId = null);

    // Add a new tracking event and update parcel's current status.
    Task<ParcelTrackingResponseDto> CreateStatusEventAsync(
        CreateStatusEventRequestDto request,
        int? createdBy);

    // Update parcel status and create a tracking event.
    Task<ParcelTrackingResponseDto> UpdateParcelStatusAsync(
        long parcelId,
        UpdateParcelStatusRequestDto request,
        int? createdBy);

    // Apply an approved Courier-only transition after checking ownership and state.
    Task<ParcelTrackingResponseDto> UpdateCourierParcelStatusAsync(
        long parcelId,
        UpdateCourierParcelStatusRequestDto request,
        int userId);
}