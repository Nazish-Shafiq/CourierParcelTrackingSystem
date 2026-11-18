
using System.Security.Claims;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelStatusDTO;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierParcelTrackingSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Booking Agent,Dispatcher,Courier")]
public class ParcelStatusController : ControllerBase
{
    private readonly IParcelStatusService _parcelStatusService;

    public ParcelStatusController(
        IParcelStatusService parcelStatusService)
    {
        _parcelStatusService = parcelStatusService;
    }

    // GET: api/ParcelStatus?page=1&pageSize=10&search=
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationRequestDto pagination,
        [FromQuery] string? search)
    {
        var result = await _parcelStatusService.GetPagedAsync(
            pagination, search);

        return Ok(
            ApiResponse<PagedResponseDto<ParcelStatusResponseDto>>.Ok(
                result,
                "Parcel statuses retrieved successfully."));
    }

    // GET: api/ParcelStatus/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var parcelStatus =
            await _parcelStatusService.GetByIdAsync(id);

        if (parcelStatus == null)
        {
            return NotFound(
                ApiResponse<ParcelStatusResponseDto>.Fail(
                    "Parcel status not found."));
        }

        return Ok(
            ApiResponse<ParcelStatusResponseDto>.Ok(
                parcelStatus,
                "Parcel status retrieved successfully."));
    }

    // POST: api/ParcelStatus
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateParcelStatusRequestDto request)
    {
        int? createdBy = GetCurrentUserId();

        var parcelStatus =
            await _parcelStatusService.CreateAsync(
                request, createdBy);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<ParcelStatusResponseDto>.Ok(
                parcelStatus,
                "Parcel status created successfully."));
    }

    // PUT: api/ParcelStatus/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateParcelStatusRequestDto request)
    {
        int? updatedBy = GetCurrentUserId();

        var parcelStatus =
            await _parcelStatusService.UpdateAsync(
                id, request, updatedBy);

        if (parcelStatus == null)
        {
            return NotFound(
                ApiResponse<ParcelStatusResponseDto>.Fail(
                    "Parcel status not found."));
        }

        return Ok(
            ApiResponse<ParcelStatusResponseDto>.Ok(
                parcelStatus,
                "Parcel status updated successfully."));
    }

    // DELETE: api/ParcelStatus/1
    // Performs soft delete instead of permanently deleting.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        int? updatedBy = GetCurrentUserId();

        bool deleted =
            await _parcelStatusService.SoftDeleteAsync(
                id, updatedBy);

        if (!deleted)
        {
            return NotFound(
                ApiResponse<bool>.Fail(
                    "Parcel status not found."));
        }

        return Ok(
            ApiResponse<bool>.Ok(
                true,
                "Parcel status deleted successfully."));
    }

    // Get logged-in user ID from JWT token.
    private int? GetCurrentUserId()
    {
        string? userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(userId, out int id)
            ? id
            : null;
    }
}
