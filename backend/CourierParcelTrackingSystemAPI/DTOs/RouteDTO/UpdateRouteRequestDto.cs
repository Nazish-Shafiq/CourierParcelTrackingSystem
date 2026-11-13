
using System.ComponentModel.DataAnnotations;

namespace CourierParcelTrackingSystem.API.DTOs.RouteDTO;

public class UpdateRouteRequestDto
{
    [Required]
    [StringLength(100)]
    public string RouteName { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int OriginHubId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int DestinationHubId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? DistanceKm { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? EstimatedHours { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}
