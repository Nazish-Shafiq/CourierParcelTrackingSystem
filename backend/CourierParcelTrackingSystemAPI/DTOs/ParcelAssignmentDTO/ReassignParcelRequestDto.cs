
namespace CourierParcelTrackingSystem.API.DTOs.ParcelAssignmentDTO;

public class ReassignParcelRequestDto
{
    public int NewCourierId { get; set; }

    public string? AssignmentNotes { get; set; }
}
