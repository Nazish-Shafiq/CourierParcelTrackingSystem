
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IParcelAssignmentRepository
{
    // Get all assignments with pagination.
    Task<PagedResponseDto<ParcelAssignment>> GetPagedAsync(
        PaginationRequestDto request);

    // Get an assignment by its ID.
    Task<ParcelAssignment?> GetByIdAsync(long assignmentId);

    // Get all currently active assignments.
    Task<List<ParcelAssignment>> GetActiveAssignmentsAsync();

    // Get a parcel by ID for assignment.
    Task<Parcel?> GetParcelForAssignmentAsync(long parcelId);

    // Get a courier by ID for assignment.
    Task<Courier?> GetCourierForAssignmentAsync(int courierId);

    // Check whether a parcel already has an active assignment.
    Task<bool> HasActiveAssignmentAsync(long parcelId);

    // Check whether a courier already has an active assignment.
    Task<bool> HasActiveAssignmentForCourierAsync(int courierId);

    // Add a new assignment.
    Task AddAsync(ParcelAssignment assignment);

    // Save database changes.
    Task SaveChangesAsync();
}
