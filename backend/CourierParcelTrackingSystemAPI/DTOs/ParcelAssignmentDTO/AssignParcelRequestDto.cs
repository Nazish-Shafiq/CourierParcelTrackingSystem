
namespace CourierParcelTrackingSystem.API.DTOs.ParcelAssignmentDTO;
public class AssignParcelRequestDto
{
    public long ParcelId { get; set; }

    public int CourierId { get; set; }

    public string? AssignmentNotes { get; set; }
}
