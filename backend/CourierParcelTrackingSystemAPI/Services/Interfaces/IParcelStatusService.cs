
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelStatusDTO;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface IParcelStatusService
{
    Task<PagedResponseDto<ParcelStatusResponseDto>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search);

    Task<ParcelStatusResponseDto?> GetByIdAsync(int id);

    Task<ParcelStatusResponseDto> CreateAsync(
        CreateParcelStatusRequestDto request,
        int? createdBy);

    Task<ParcelStatusResponseDto?> UpdateAsync(
        int id,
        UpdateParcelStatusRequestDto request,
        int? updatedBy);

    Task<bool> SoftDeleteAsync(int id, int? updatedBy);
}
