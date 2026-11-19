
using CourierParcelTrackingSystem.API.DTOs.Common;
using RouteEntity = CourierParcelTrackingSystem.API.Entities.Route;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IRouteRepository
{
    Task<PagedResponseDto<RouteEntity>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search,
        bool? isActive);

    Task<RouteEntity?> GetByIdAsync(int id);

    Task<bool> HubExistsAsync(int hubId);

    Task AddAsync(RouteEntity route);

    Task UpdateAsync(RouteEntity route);

    Task SoftDeleteAsync(RouteEntity route);
}
