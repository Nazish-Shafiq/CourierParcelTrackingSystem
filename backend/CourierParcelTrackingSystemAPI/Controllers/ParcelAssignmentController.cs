
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelDTO;
using CourierParcelTrackingSystem.API.DTOs.ParcelAssignmentDTO;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using CourierParcelTrackingSystem.API.Authorization;

namespace CourierParcelTrackingSystem.API.Controllers;

[Route("api/parcel-assignments")]
[ApiController]
[Authorize]
public class ParcelAssignmentController : ControllerBase
{
    private readonly IParcelAssignmentService _service;

    public ParcelAssignmentController(IParcelAssignmentService service)
    {
        _service = service;
    }

    // Get the logged-in user's ID from JWT claims.
    private int? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userId, out var id) ? id : null;
    }

    // POST: api/parcel-assignments
    // Assign a parcel to a courier.
    [HttpPost]
    [ScreenAccess("/dispatch/assign")]
    public async Task<ActionResult<ApiResponse<ParcelAssignmentResponseDto>>> AssignParcel(
        [FromBody] AssignParcelRequestDto request)
    {
        var result = await _service.AssignParcelAsync(
            request,
            GetCurrentUserId());

        var response = ApiResponse<ParcelAssignmentResponseDto>.Ok(
            result,
            "Parcel assigned successfully.");

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.ParcelAssignmentId },
            response);
    }

    // GET: api/parcel-assignments
    // Get all assignments with pagination.
    [HttpGet]
    [ScreenAccess("/dispatch/assignments")]
    public async Task<ActionResult<ApiResponse<PagedResponseDto<ParcelAssignmentResponseDto>>>> GetAll(
        [FromQuery] PaginationRequestDto request,
        [FromQuery] bool? activeOnly)
    {
        var result = await _service.GetPagedAsync(request, activeOnly);

        return Ok(ApiResponse<PagedResponseDto<ParcelAssignmentResponseDto>>.Ok(
            result,
            "Parcel assignments retrieved successfully."));
    }

    // GET: api/parcel-assignments/{id}
    // Get assignment details by ID.
    [HttpGet("{id:long}")]
    [ScreenAccess("/dispatch/assignments", "/dispatch/assign")]
    public async Task<ActionResult<ApiResponse<ParcelAssignmentResponseDto>>> GetById(
        long id)
    {
        var result = await _service.GetByIdAsync(id);

        return Ok(ApiResponse<ParcelAssignmentResponseDto>.Ok(
            result,
            "Parcel assignment retrieved successfully."));
    }

    // GET: api/parcel-assignments/active
    // Get all active assignments.
    [HttpGet("active")]
    [ScreenAccess("/dispatch/assignments", "/dispatch/assign")]
    public async Task<ActionResult<ApiResponse<List<ParcelAssignmentResponseDto>>>> GetActive()
    {
        var result = await _service.GetActiveAssignmentsAsync();

        return Ok(ApiResponse<List<ParcelAssignmentResponseDto>>.Ok(
            result,
            "Active parcel assignments retrieved successfully."));
    }

    // GET: api/parcel-assignments/eligible-parcels
    // Get parcels eligible for courier assignment.
    [HttpGet("eligible-parcels")]
    [ScreenAccess("/dispatch/assign", "/dispatch/unassigned-parcels")]
    public async Task<ActionResult<ApiResponse<List<ParcelResponseDto>>>>
        GetEligibleParcelsForInitialAssignment()
    {
        var result = await _service.GetEligibleParcelsForInitialAssignmentAsync();

        return Ok(ApiResponse<List<ParcelResponseDto>>.Ok(
            result,
            "Parcels eligible for courier assignment retrieved successfully."));
    }

    // PUT: api/parcel-assignments/{id}/reassign
    // Reassign a parcel to another courier.
    [HttpPut("{id:long}/reassign")]
    [ScreenAccess("/dispatch/assignments", "/dispatch/assign")]
    public async Task<ActionResult<ApiResponse<ParcelAssignmentResponseDto>>> ReassignParcel(
        long id,
        [FromBody] ReassignParcelRequestDto request)
    {
        var result = await _service.ReassignParcelAsync(
            id,
            request,
            GetCurrentUserId());

        return Ok(ApiResponse<ParcelAssignmentResponseDto>.Ok(
            result,
            "Parcel reassigned successfully."));
    }

    // PUT: api/parcel-assignments/{id}/unassign
    // Unassign a courier from a parcel.
    [HttpPut("{id:long}/unassign")]
    [ScreenAccess("/dispatch/assignments", "/dispatch/assign")]
    public async Task<ActionResult<ApiResponse<string>>> UnassignParcel(
        long id)
    {
        await _service.UnassignParcelAsync(
            id,
            GetCurrentUserId());

        return Ok(ApiResponse<string>.Ok(
            "Unassigned",
            "Courier unassigned successfully."));
    }
}
