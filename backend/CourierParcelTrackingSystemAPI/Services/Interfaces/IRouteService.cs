
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.RouteDTO;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface IRouteService
{
    Task<PagedResponseDto<RouteResponseDto>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search);

    Task<RouteResponseDto?> GetByIdAsync(int id);

    Task<RouteResponseDto> CreateAsync(
        CreateRouteRequestDto request,
        int? createdBy);

    Task<RouteResponseDto?> UpdateAsync(
        int id,
        UpdateRouteRequestDto request,
        int? updatedBy);

    Task<bool> SoftDeleteAsync(int id, int? updatedBy);
}
