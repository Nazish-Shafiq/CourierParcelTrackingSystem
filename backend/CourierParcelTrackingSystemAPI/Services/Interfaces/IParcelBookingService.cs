using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelBookingDTO;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface IParcelBookingService
{
    Task<PagedResponseDto<ParcelBookingResponseDto>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search);

    Task<ParcelBookingResponseDto?> GetByIdAsync(long id);

    Task<ParcelBookingResponseDto> CreateBookingAsync(
        CreateParcelBookingRequestDto request,
        int? createdBy);
}