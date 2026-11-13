
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.CouriersDTO;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface ICourierService
{
    // Get couriers with pagination and search.
    Task<PagedResponseDto<CourierResponseDto>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search);

    // Get courier by ID.
    Task<CourierResponseDto?> GetByIdAsync(int id);

    // Create a new courier.
    Task<CourierResponseDto> CreateAsync(
        CreateCourierRequestDto request,
        int? createdBy);

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
