namespace CourierParcelTrackingSystem.API.DTOs.ParcelBookingDTO;

public class ParcelRecipientResponseDto
{
    public long ParcelRecipientId { get; set; }

    public string RecipientName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string AddressLine { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string? Province { get; set; }

    public string? PostalCode { get; set; }
}