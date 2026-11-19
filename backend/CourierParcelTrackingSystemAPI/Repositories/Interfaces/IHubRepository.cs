
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IHubRepository
{
    // Get hubs with pagination and optional search.
    Task<PagedResponseDto<Hub>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search,
        bool? activeOnly = null,
        bool? isActive = null);

    // Get a hub by ID.
    Task<Hub?> GetByIdAsync(int id);

    // Check whether a hub code already exists.
    Task<bool> HubCodeExistsAsync(
        string hubCode,
        int? excludeHubId = null);

    // Add a new hub.
    Task AddAsync(Hub hub);

    // Update an existing hub.
    Task UpdateAsync(Hub hub);

    // Soft delete a hub.
    Task SoftDeleteAsync(Hub hub);
}
