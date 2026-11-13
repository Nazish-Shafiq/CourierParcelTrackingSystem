
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.RouteDTO;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using RouteEntity = CourierParcelTrackingSystem.API.Entities.Route;

namespace CourierParcelTrackingSystem.API.Services;

public class RouteService : IRouteService
{
    private readonly IRouteRepository _routeRepository;

    public RouteService(IRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
    }

    // Get paginated routes.
    public async Task<PagedResponseDto<RouteResponseDto>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search)
    {
        var result = await _routeRepository.GetPagedAsync(
            pagination, search);

        return new PagedResponseDto<RouteResponseDto>
        {
            Items = result.Items.Select(MapToResponse).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages
        };
    }

    // Get route by ID.
    public async Task<RouteResponseDto?> GetByIdAsync(int id)
    {
        var route = await _routeRepository.GetByIdAsync(id);

        return route == null ? null : MapToResponse(route);
    }

    // Create a new route.
    public async Task<RouteResponseDto> CreateAsync(
        CreateRouteRequestDto request,
        int? createdBy)
    {
        ValidateDifferentHubs(
            request.OriginHubId,
            request.DestinationHubId);

        await ValidateHubAsync(request.OriginHubId);
        await ValidateHubAsync(request.DestinationHubId);

        var route = new RouteEntity
        {
            RouteCode = GenerateRouteCode(),
            RouteName = request.RouteName.Trim(),
            OriginHubId = request.OriginHubId,
            DestinationHubId = request.DestinationHubId,
            DistanceKm = request.DistanceKm,
            EstimatedHours = request.EstimatedHours,
            Description = Normalize(request.Description),
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _routeRepository.AddAsync(route);

        var createdRoute = await _routeRepository.GetByIdAsync(
            route.RouteId);

        if (createdRoute == null)
        {
            throw new InvalidOperationException(
                "Route was created but could not be retrieved.");
        }

        return MapToResponse(createdRoute);
    }

    // Update an existing route.
    public async Task<RouteResponseDto?> UpdateAsync(
        int id,
        UpdateRouteRequestDto request,
        int? updatedBy)
    {
        var route = await _routeRepository.GetByIdAsync(id);

        if (route == null)
        {
            return null;
        }

        ValidateDifferentHubs(
            request.OriginHubId,
            request.DestinationHubId);

        await ValidateHubAsync(request.OriginHubId);
        await ValidateHubAsync(request.DestinationHubId);

        route.RouteName = request.RouteName.Trim();
        route.OriginHubId = request.OriginHubId;
        route.DestinationHubId = request.DestinationHubId;
        route.DistanceKm = request.DistanceKm;
        route.EstimatedHours = request.EstimatedHours;
        route.Description = Normalize(request.Description);
        route.IsActive = request.IsActive;
        route.UpdatedAt = DateTime.UtcNow;
        route.UpdatedBy = updatedBy;

        await _routeRepository.UpdateAsync(route);

        // Retrieve the updated route and its hub details.
        // The repository returns the tracked entity in this scope.
        return MapToResponse(route);
    }

    // Soft delete an existing route.
    public async Task<bool> SoftDeleteAsync(int id, int? updatedBy)
    {
        var route = await _routeRepository.GetByIdAsync(id);

        if (route == null)
        {
            return false;
        }

        route.IsActive = false;
        route.IsDeleted = true;
        route.UpdatedAt = DateTime.UtcNow;
        route.UpdatedBy = updatedBy;

        await _routeRepository.SoftDeleteAsync(route);

        return true;
    }

    // Origin and destination cannot be the same.
    private static void ValidateDifferentHubs(
        int originHubId,
        int destinationHubId)
    {
        if (originHubId == destinationHubId)
        {
            throw new ArgumentException(
                "Origin hub and destination hub must be different.");
        }
    }

    // Validate selected hub.
    private async Task ValidateHubAsync(int hubId)
    {
        bool exists = await _routeRepository.HubExistsAsync(hubId);

        if (!exists)
        {
            throw new ArgumentException(
                $"Hub with ID {hubId} does not exist, is inactive, or is deleted.");
        }
    }

    // Generate unique route code.
    private static string GenerateRouteCode()
    {
        return $"RTE-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }

    // Normalize optional strings.
    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    // Map entity to response DTO.
    private static RouteResponseDto MapToResponse(RouteEntity route)
    {
        return new RouteResponseDto
        {
            RouteId = route.RouteId,
            RouteCode = route.RouteCode,
            RouteName = route.RouteName,
            OriginHubId = route.OriginHubId,
            OriginHubName = route.OriginHub.HubName,
            DestinationHubId = route.DestinationHubId,
            DestinationHubName = route.DestinationHub.HubName,
            DistanceKm = route.DistanceKm,
            EstimatedHours = route.EstimatedHours,
            Description = route.Description,
            IsActive = route.IsActive,
            CreatedAt = route.CreatedAt
        };
    }
}
