
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelAssignmentDTO;


namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface IParcelAssignmentService
{
    Task<ParcelAssignmentResponseDto> AssignParcelAsync(
        AssignParcelRequestDto request,
        int? assignedBy);

    Task<PagedResponseDto<ParcelAssignmentResponseDto>> GetPagedAsync(
        PaginationRequestDto request);

    Task<ParcelAssignmentResponseDto> GetByIdAsync(long assignmentId);

    Task<List<ParcelAssignmentResponseDto>> GetActiveAssignmentsAsync();

    Task<ParcelAssignmentResponseDto> ReassignParcelAsync(
     long assignmentId,
     ReassignParcelRequestDto request,
     int? reassignedBy);

    Task UnassignParcelAsync(long assignmentId, int? unassignedBy);
}