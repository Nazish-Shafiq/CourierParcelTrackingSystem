
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface ICourierRepository
{
    // Get couriers with pagination and optional search.
    Task<PagedResponseDto<Courier>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search,
        bool? isAvailable);
    Task<List<Courier>> GetActiveCouriersForDispatchAsync(int hubId);
    // Get courier by ID.
    Task<Courier?> GetByIdAsync(int id);

    // Check duplicate employee code.
    Task<bool> EmployeeCodeExistsAsync(
        string employeeCode,
        int? excludeCourierId = null);
    Task<string> GenerateUniqueEmployeeCodeAsync();
    Task<bool> IsActiveCourierUserAsync(int userId);
    Task<bool> UserHasCourierProfileAsync(int userId, int? excludeCourierId = null);
    Task<bool> HasActiveAssignmentAsync(int courierId);

    // Update an existing courier.
    Task UpdateAsync(Courier courier);

    // Soft delete a courier.
    Task SoftDeleteAsync(Courier courier);
}
