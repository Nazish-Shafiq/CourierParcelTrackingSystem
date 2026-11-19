
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.CouriersDTO;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface ICourierService
{
    // Get couriers with pagination and search.
    Task<PagedResponseDto<CourierResponseDto>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search,
        bool? isAvailable,
        bool? isActive);
    Task<List<CourierDispatchOptionDto>> GetDispatchOptionsAsync(long parcelId);
    // Get courier by ID.
    Task<CourierResponseDto?> GetByIdAsync(int id);

    // Update an existing courier.
    Task<CourierResponseDto?> UpdateAsync(
        int id,
        UpdateCourierRequestDto request,
        int? updatedBy);

    // Soft delete a courier.
    Task<bool> SoftDeleteAsync(
        int id,
        int? updatedBy);
}
