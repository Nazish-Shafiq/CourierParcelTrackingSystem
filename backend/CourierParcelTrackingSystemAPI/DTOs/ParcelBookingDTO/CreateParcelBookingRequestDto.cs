using System.ComponentModel.DataAnnotations;

namespace CourierParcelTrackingSystem.API.DTOs.ParcelBookingDTO;

public class CreateParcelBookingRequestDto
{
    // Existing customer who is booking the parcel.
    [Required]
    [Range(1, int.MaxValue)]
    public int SenderCustomerId { get; set; }

    // Route is optional during booking.
    [Range(1, int.MaxValue)]
    public int? RouteId { get; set; }

    // Parcel information.
    [Required]
    [Range(typeof(decimal), "0.01", "9999999.999")]
    public decimal TotalWeightKg { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal? DeclaredValue { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }

    [StringLength(500)]
    public string? SpecialInstructions { get; set; }

    // Recipient information.
    [Required]
    [StringLength(150)]
    public string RecipientName { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string RecipientPhone { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(150)]
    public string? RecipientEmail { get; set; }

    [Required]
    [StringLength(250)]
    public string RecipientAddressLine { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string RecipientCity { get; set; } = string.Empty;

    [StringLength(100)]
    public string? RecipientProvince { get; set; }

    [StringLength(20)]
    public string? RecipientPostalCode { get; set; }
}
