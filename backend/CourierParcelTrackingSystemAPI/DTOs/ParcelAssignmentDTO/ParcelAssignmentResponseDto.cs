
namespace CourierParcelTrackingSystem.API.DTOs.ParcelAssignmentDTO;

public class ParcelAssignmentResponseDto
{
    public long ParcelAssignmentId { get; set; }

    public long ParcelId { get; set; }

    public string TrackingNumber { get; set; } = string.Empty;

    public int CourierId { get; set; }

    public string CourierName { get; set; } = string.Empty;

    public string EmployeeCode { get; set; } = string.Empty;

    public DateTime AssignedAt { get; set; }

    public int? AssignedBy { get; set; }

    public DateTime? UnassignedAt { get; set; }

    public int? UnassignedBy { get; set; }

    public string? AssignmentNotes { get; set; }

    public bool IsActiveAssignment => UnassignedAt == null;
}
