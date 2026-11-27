using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelBookingDTO;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface IParcelBookingService
{
    Task<PagedResponseDto<ParcelBookingResponseDto>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search,
        string? filter,
        DateTime? fromBookingDate,
        DateTime? beforeBookingDate);

    Task<ParcelBookingResponseDto?> GetByIdAsync(long id);

    Task<ParcelBookingResponseDto> CreateBookingAsync(
        CreateParcelBookingRequestDto request,
        int? createdBy,
        string? idempotencyKey);
}