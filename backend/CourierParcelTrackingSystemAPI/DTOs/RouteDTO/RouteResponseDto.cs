
namespace CourierParcelTrackingSystem.API.DTOs.RouteDTO;

public class RouteResponseDto
{
    public int RouteId { get; set; }

    public string RouteCode { get; set; } = string.Empty;

    public string RouteName { get; set; } = string.Empty;

    public int OriginHubId { get; set; }

    public string OriginHubName { get; set; } = string.Empty;

    public int DestinationHubId { get; set; }

    public string DestinationHubName { get; set; } = string.Empty;

    public decimal? DistanceKm { get; set; }

    public decimal? EstimatedHours { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
