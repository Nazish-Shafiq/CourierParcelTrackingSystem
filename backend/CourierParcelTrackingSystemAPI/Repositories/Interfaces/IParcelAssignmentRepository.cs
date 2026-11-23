
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IParcelAssignmentRepository
{
    // Get all assignments with pagination.
    Task<PagedResponseDto<ParcelAssignment>> GetPagedAsync(
        PaginationRequestDto request,
        bool? activeOnly);

    // Get an assignment by its ID.
    Task<ParcelAssignment?> GetByIdAsync(long assignmentId);

    // Get all currently active assignments.
    Task<List<ParcelAssignment>> GetActiveAssignmentsAsync();

    // Get courier IDs that currently have an active assignment.
    Task<List<int>> GetActiveCourierIdsAsync();

    // Get parcels eligible for courier assignment.
    Task<List<Parcel>> GetEligibleParcelsForInitialAssignmentAsync();

    // Get a parcel by ID for assignment.
    Task<Parcel?> GetParcelForAssignmentAsync(long parcelId);

    // Get a courier by ID for assignment.
    Task<Courier?> GetCourierForAssignmentAsync(int courierId);

    // Check whether a parcel already has an active assignment.
    Task<bool> HasActiveAssignmentAsync(long parcelId);

    // Recheck assignment eligibility immediately before saving.
    Task<bool> IsEligibleForInitialAssignmentAsync(long parcelId);

    // Check whether a courier already has an active assignment.
    Task<bool> HasActiveAssignmentForCourierAsync(int courierId);

    // Check whether the courier has another active assignment besides the given one.
    Task<bool> HasOtherActiveAssignmentForCourierAsync(
        int courierId,
        long excludedAssignmentId);

    // Get the latest failed-delivery event time for a parcel.
    Task<DateTime?> GetLatestDeliveryAttemptAtAsync(long parcelId);

    // Add a new assignment.
    Task AddAsync(ParcelAssignment assignment);

    // Save database changes.
    Task SaveChangesAsync();
}
