
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
    [ProducesResponseType(
        typeof(ApiResponse<PagedResponseDto<CourierResponseDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCouriers(
        [FromQuery] PaginationRequestDto pagination,
        [FromQuery] string? search)
    {
        var result = await _service.GetPagedAsync(pagination, search);

        return Ok(
            ApiResponse<PagedResponseDto<CourierResponseDto>>.Ok(
                result,
                "Couriers retrieved successfully."
            )
        );
    }

    // Get courier by ID.
    [HttpGet("{id:int}")]
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

    // Create a new courier.
    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponse<CourierResponseDto>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCourierRequestDto request)
    {
        // Get logged-in user's ID from JWT token.
        int? createdBy = GetCurrentUserId();

        var result = await _service.CreateAsync(request, createdBy);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.CourierId },
            ApiResponse<CourierResponseDto>.Ok(
                result,
                "Courier created successfully."
            )
        );
    }

    // Update an existing courier.
    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<CourierResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCourierRequestDto request)
    {
        // Get logged-in user's ID from JWT token.
        int? updatedBy = GetCurrentUserId();

        var result = await _service.UpdateAsync(
            id,
            request,
            updatedBy);

        if (result == null)
        {
            return NotFound(
                ApiResponse<CourierResponseDto>.Fail(
                    "Courier not found."));
        }

        return Ok(
            ApiResponse<CourierResponseDto>.Ok(
                result,
                "Courier updated successfully."
            )
        );
    }

    // Soft delete a courier.
    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<bool>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        // Get logged-in user's ID from JWT token.
        int? updatedBy = GetCurrentUserId();

        var result = await _service.SoftDeleteAsync(
            id,
            updatedBy);

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
