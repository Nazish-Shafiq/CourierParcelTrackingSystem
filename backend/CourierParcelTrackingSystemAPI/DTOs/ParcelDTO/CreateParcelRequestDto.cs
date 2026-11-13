using System.ComponentModel.DataAnnotations;

namespace CourierParcelTrackingSystem.API.DTOs.ParcelDTO;

public class CreateParcelRequestDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int SenderCustomerId { get; set; }

    [Range(1, int.MaxValue)]
    public int? RouteId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int CurrentStatusId { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }

    [Range(typeof(decimal), "0.001", "9999999")]
    public decimal TotalWeightKg { get; set; }

    [Range(typeof(decimal), "0", "9999999999")]
    public decimal? DeclaredValue { get; set; }

    [StringLength(500)]
    public string? SpecialInstructions { get; set; }
}
