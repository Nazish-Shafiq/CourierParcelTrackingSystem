
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.HubDTO;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface IHubService
{
    // Get hubs with pagination and search.
    Task<PagedResponseDto<HubResponseDto>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search);

    // Get hub by ID.
    Task<HubResponseDto?> GetByIdAsync(int id);

    // Create a new hub.
    Task<HubResponseDto> CreateAsync(
        CreateHubRequestDto request,
        int? createdBy);

    // Update an existing hub.
    Task<HubResponseDto?> UpdateAsync(
        int id,
        UpdateHubRequestDto request,
        int? updatedBy);

    // Soft delete a hub.
    Task<bool> SoftDeleteAsync(
        int id,
        int? updatedBy);
}
