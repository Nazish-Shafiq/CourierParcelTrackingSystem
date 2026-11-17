
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelAssignmentDTO;
using CourierParcelTrackingSystem.API.Services.Interfaces;

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
    [Authorize(Roles = "Admin,Dispatcher")]
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
    public async Task<ActionResult<ApiResponse<PagedResponseDto<ParcelAssignmentResponseDto>>>> GetAll(
        [FromQuery] PaginationRequestDto request)
    {
        var result = await _service.GetPagedAsync(request);

        return Ok(ApiResponse<PagedResponseDto<ParcelAssignmentResponseDto>>.Ok(
            result,
            "Parcel assignments retrieved successfully."));
    }

    // GET: api/parcel-assignments/{id}
    // Get assignment details by ID.
    [HttpGet("{id:long}")]
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
    public async Task<ActionResult<ApiResponse<List<ParcelAssignmentResponseDto>>>> GetActive()
    {
        var result = await _service.GetActiveAssignmentsAsync();

        return Ok(ApiResponse<List<ParcelAssignmentResponseDto>>.Ok(
            result,
            "Active parcel assignments retrieved successfully."));
    }

    // PUT: api/parcel-assignments/{id}/reassign
    // Reassign a parcel to another courier.
    [HttpPut("{id:long}/reassign")]
    [Authorize(Roles = "Admin,Dispatcher")]
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
    [Authorize(Roles = "Admin,Dispatcher")]
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
