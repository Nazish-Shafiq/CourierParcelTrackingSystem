
using System.Security.Claims;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.CouriersDTO;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierParcelTrackingSystem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/couriers")]
public class CourierController : ControllerBase
{
    private readonly ICourierService _service;

    public CourierController(ICourierService service)
    {
        _service = service;
    }

    // Get all couriers with pagination and optional search.
    [HttpGet]
    [Authorize(Roles = "Admin,Dispatcher")]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResponseDto<CourierResponseDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCouriers(
        [FromQuery] PaginationRequestDto pagination,
        [FromQuery] string? search,
        [FromQuery] bool? isAvailable,
        [FromQuery] bool? isActive)
    {
        var result = await _service.GetPagedAsync(
            pagination, search, isAvailable, isActive);

        return Ok(
            ApiResponse<PagedResponseDto<CourierResponseDto>>.Ok(
                result,
                "Couriers retrieved successfully."
            )
        );
    }

    // Get active couriers and dispatch-relevant details for Dispatcher screens.
    [HttpGet("dispatch-options")]
    [Authorize(Roles = "Dispatcher")]
    [ProducesResponseType(
        typeof(ApiResponse<List<CourierDispatchOptionDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDispatchOptions(
        [FromQuery] long parcelId)
    {
        if (parcelId <= 0)
        {
            return BadRequest(
                ApiResponse<List<CourierDispatchOptionDto>>.Fail(
                    "A valid parcelId is required to load eligible couriers."));
        }

        var result = await _service.GetDispatchOptionsAsync(parcelId);

        return Ok(
            ApiResponse<List<CourierDispatchOptionDto>>.Ok(
                result,
                "Courier dispatch options retrieved successfully."
            )
        );
    }

    // Get courier by ID.
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(
        typeof(ApiResponse<CourierResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        return result is null
            ? NotFound(
                ApiResponse<CourierResponseDto>.Fail(
                    "Courier not found."))
            : Ok(
                ApiResponse<CourierResponseDto>.Ok(result));
    }

    // Update an existing courier.
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(
        typeof(ApiResponse<CourierResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCourierRequestDto request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request, GetCurrentUserId());
            if (result == null)
                return NotFound(ApiResponse<CourierResponseDto>.Fail("Courier not found."));

            return Ok(ApiResponse<CourierResponseDto>.Ok(result, "Courier updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<CourierResponseDto>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<CourierResponseDto>.Fail(ex.Message));
        }
    }

    // Soft delete a courier.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(
        typeof(ApiResponse<bool>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        // Get logged-in user's ID from JWT token.
        int? updatedBy = GetCurrentUserId();

        bool result;
        try
        {
            result = await _service.SoftDeleteAsync(id, updatedBy);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<bool>.Fail(ex.Message));
        }

        if (!result)
        {
            return NotFound(
                ApiResponse<bool>.Fail(
                    "Courier not found."));
        }

        return Ok(
            ApiResponse<bool>.Ok(
                true,
                "Courier deleted successfully."
            )
        );
    }

    // Extract logged-in user's ID from JWT claims.
    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(userIdClaim, out var userId)
            ? userId
            : null;
    }
}
