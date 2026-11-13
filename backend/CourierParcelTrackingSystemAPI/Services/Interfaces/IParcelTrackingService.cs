
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelTracking;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface IParcelTrackingService
{
    // Get tracking events with pagination.
    Task<PagedResponseDto<ParcelTrackingResponseDto>> GetPagedAsync(
        PaginationRequestDto request);

    // Get complete tracking history by tracking number.
    Task<List<ParcelTrackingResponseDto>> GetTrackingHistoryAsync(
        string trackingNumber);

    // Add a new tracking event and update parcel's current status.
    Task<ParcelTrackingResponseDto> CreateStatusEventAsync(
        CreateStatusEventRequestDto request,
        int? createdBy);

    // Update parcel status and create a tracking event.
    Task<ParcelTrackingResponseDto> UpdateParcelStatusAsync(
        long parcelId,
        UpdateParcelStatusRequestDto request,
        int? createdBy);
}