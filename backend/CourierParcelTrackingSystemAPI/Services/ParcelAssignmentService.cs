
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelAssignmentDTO;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;

namespace CourierParcelTrackingSystem.API.Services;

public class ParcelAssignmentService : IParcelAssignmentService
{
    private readonly IParcelAssignmentRepository _repository;

    public ParcelAssignmentService(
        IParcelAssignmentRepository repository)
    {
        _repository = repository;
    }

    // Assign a parcel to an available courier.
    public async Task<ParcelAssignmentResponseDto> AssignParcelAsync(
        AssignParcelRequestDto request,
        int? assignedBy)
    {
        var parcel = await _repository.GetParcelForAssignmentAsync(
            request.ParcelId);

        if (parcel == null)
            throw new KeyNotFoundException(
                "Parcel not found or inactive.");

        ParcelAssignmentHubResolver.EnsureParcelCanBeAssigned(parcel);

        if (await _repository.HasActiveAssignmentAsync(request.ParcelId))
            throw new InvalidOperationException(
                "This parcel already has an active assignment.");

        var courier = await _repository.GetCourierForAssignmentAsync(
            request.CourierId);

        if (courier == null)
            throw new KeyNotFoundException(
                "Courier not found or inactive.");

        EnsureCourierMatchesOperationalHub(parcel, courier);

        if (!courier.IsAvailable)
            throw new InvalidOperationException(
                "The selected courier is not available.");

        if (await _repository.HasActiveAssignmentForCourierAsync(
            request.CourierId))
        {
            throw new InvalidOperationException(
                "The selected courier already has an active assignment.");
        }

        var assignment = new ParcelAssignment
        {
            ParcelId = parcel.ParcelId,
            CourierId = courier.CourierId,
            AssignedAt = await GetAssignmentTimeAsync(
                parcel.ParcelId,
                parcel.CurrentStatus.StatusCode),
            AssignedBy = assignedBy,
            AssignmentNotes = request.AssignmentNotes,
            Parcel = parcel,
            Courier = courier
        };

        courier.IsAvailable = false;

        await _repository.AddAsync(assignment);
        await _repository.SaveChangesAsync();

        return MapToResponse(assignment);
    }

    // Get all assignments with pagination.
    public async Task<PagedResponseDto<ParcelAssignmentResponseDto>>
        GetPagedAsync(PaginationRequestDto request, bool? activeOnly)
    {
        var result = await _repository.GetPagedAsync(request, activeOnly);

        return new PagedResponseDto<ParcelAssignmentResponseDto>
        {
            Items = result.Items.Select(MapToResponse).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages
        };
    }

    // Get assignment details by ID.
    public async Task<ParcelAssignmentResponseDto> GetByIdAsync(
        long assignmentId)
    {
        var assignment = await _repository.GetByIdAsync(assignmentId);

        if (assignment == null)
            throw new KeyNotFoundException(
                "Parcel assignment not found.");

        return MapToResponse(assignment);
    }

    // Get all currently active assignments.
    public async Task<List<ParcelAssignmentResponseDto>>
        GetActiveAssignmentsAsync()
    {
        var assignments = await _repository.GetActiveAssignmentsAsync();

        return assignments.Select(MapToResponse).ToList();
    }

    // Reassign a parcel to another courier.
    public async Task<ParcelAssignmentResponseDto> ReassignParcelAsync(
        long assignmentId,
        ReassignParcelRequestDto request,
        int? reassignedBy)
    {
        var currentAssignment = await _repository.GetByIdAsync(
            assignmentId);

        if (currentAssignment == null)
            throw new KeyNotFoundException(
                "Parcel assignment not found.");

        if (currentAssignment.UnassignedAt != null)
            throw new InvalidOperationException(
                "This assignment is no longer active.");

        if (currentAssignment.Courier == null)
            throw new InvalidOperationException(
                "The current courier information could not be loaded.");

        if (currentAssignment.Parcel == null)
            throw new InvalidOperationException(
                "The parcel information could not be loaded.");

        ParcelAssignmentHubResolver.EnsureParcelCanBeAssigned(
            currentAssignment.Parcel);

        var newCourier = await _repository.GetCourierForAssignmentAsync(
            request.NewCourierId);

        if (newCourier == null)
            throw new KeyNotFoundException(
                "New courier not found or inactive.");

        EnsureCourierMatchesOperationalHub(
            currentAssignment.Parcel,
            newCourier);

        if (newCourier.CourierId == currentAssignment.CourierId)
            throw new InvalidOperationException(
                "The parcel is already assigned to this courier.");

        if (!newCourier.IsAvailable)
            throw new InvalidOperationException(
                "The selected courier is not available.");

        if (await _repository.HasActiveAssignmentForCourierAsync(
            request.NewCourierId))
        {
            throw new InvalidOperationException(
                "The selected courier already has an active assignment.");
        }

        // End the previous assignment and release the old courier.
        currentAssignment.UnassignedAt = DateTime.UtcNow;
        currentAssignment.UnassignedBy = reassignedBy;

        currentAssignment.Courier.IsAvailable = true;

        // Create a new assignment to preserve assignment history.
        var newAssignment = new ParcelAssignment
        {
            ParcelId = currentAssignment.ParcelId,
            Parcel = currentAssignment.Parcel,
            CourierId = newCourier.CourierId,
            Courier = newCourier,
            AssignedAt = await GetAssignmentTimeAsync(
                currentAssignment.ParcelId,
                currentAssignment.Parcel.CurrentStatus.StatusCode),
            AssignedBy = reassignedBy,
            AssignmentNotes = request.AssignmentNotes
        };

        newCourier.IsAvailable = false;

        await _repository.AddAsync(newAssignment);
        await _repository.SaveChangesAsync();

        return MapToResponse(newAssignment);
    }

    // Unassign a courier from a parcel.
    public async Task UnassignParcelAsync(
        long assignmentId,
        int? unassignedBy)
    {
        var assignment = await _repository.GetByIdAsync(assignmentId);

        if (assignment == null)
            throw new KeyNotFoundException(
                "Parcel assignment not found.");

        if (assignment.UnassignedAt != null)
            throw new InvalidOperationException(
                "This assignment has already been unassigned.");

        if (assignment.Courier == null)
            throw new InvalidOperationException(
                "Courier information could not be loaded.");

        assignment.UnassignedAt = DateTime.UtcNow;
        assignment.UnassignedBy = unassignedBy;

        assignment.Courier.IsAvailable = true;

        await _repository.SaveChangesAsync();
    }

    // Convert entity into response DTO.
    private static ParcelAssignmentResponseDto MapToResponse(
        ParcelAssignment assignment)
    {
        return new ParcelAssignmentResponseDto
        {
            ParcelAssignmentId = assignment.ParcelAssignmentId,
            ParcelId = assignment.ParcelId,
            TrackingNumber = assignment.Parcel?.TrackingNumber
                             ?? string.Empty,
            CourierId = assignment.CourierId,
            CourierName = assignment.Courier?.FullName
                          ?? string.Empty,
            EmployeeCode = assignment.Courier?.EmployeeCode
                           ?? string.Empty,
            AssignedAt = assignment.AssignedAt,
            AssignedBy = assignment.AssignedBy,
            UnassignedAt = assignment.UnassignedAt,
            UnassignedBy = assignment.UnassignedBy,
            AssignmentNotes = assignment.AssignmentNotes
        };
    }

    private static void EnsureCourierMatchesOperationalHub(
        Parcel parcel,
        Courier courier)
    {
        var operationalHub = ParcelAssignmentHubResolver.ResolveOperationalHub(parcel);
        if (operationalHub is null ||
            !operationalHub.IsActive ||
            operationalHub.IsDeleted)
        {
            throw new InvalidOperationException(
                "The parcel does not have an active operational hub for courier assignment.");
        }

        if (!courier.HubId.HasValue ||
            courier.Hub is null ||
            !courier.Hub.IsActive ||
            courier.Hub.IsDeleted)
        {
            throw new InvalidOperationException(
                "The selected courier must be assigned to an active hub.");
        }

        if (courier.HubId != operationalHub.HubId)
            throw new InvalidOperationException(
                "The selected courier is not assigned to the parcel's operational hub.");
    }

    private async Task<DateTime> GetAssignmentTimeAsync(
        long parcelId,
        string currentStatusCode)
    {
        var now = DateTime.UtcNow;
        if (!currentStatusCode.Equals(
            "DELIVERY_ATTEMPTED",
            StringComparison.OrdinalIgnoreCase))
        {
            return now;
        }

        var latestAttemptAt = await _repository
            .GetLatestDeliveryAttemptAtAsync(parcelId);
        if (!latestAttemptAt.HasValue)
        {
            throw new InvalidOperationException(
                "A delivery attempt event is required before this parcel can be retried.");
        }

        // Assignment timestamps use second precision, so advance beyond the
        // stored event time when a reassignment happens within the same second.
        var afterAttempt = latestAttemptAt.Value.AddSeconds(1);
        return now > afterAttempt ? now : afterAttempt;
    }
}