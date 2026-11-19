
using System.Security.Claims;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.HubDTO;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierParcelTrackingSystem.API.Controllers;

[Authorize(Roles = "Admin,Booking Agent,Dispatcher,Courier")]
[ApiController]
[Route("api/hubs")]
public class HubController : ControllerBase
{
    private readonly IHubService _service;

    public HubController(IHubService service)
    {
        _service = service;
    }

    // Get all hubs with pagination and optional search.
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResponseDto<HubResponseDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHubs(
        [FromQuery] PaginationRequestDto pagination,
        [FromQuery] string? search,
        [FromQuery] bool? activeOnly,
        [FromQuery] bool? isActive)
    {
        var result = await _service.GetPagedAsync(
            pagination, search, activeOnly, isActive);

        return Ok(
            ApiResponse<PagedResponseDto<HubResponseDto>>.Ok(
                result,
                "Hubs retrieved successfully."
            )
        );
    }

    // Get hub by ID.
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<HubResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        return result is null
            ? NotFound(
                ApiResponse<HubResponseDto>.Fail(
                    "Hub not found."))
            : Ok(
                ApiResponse<HubResponseDto>.Ok(result));
    }

    // Create a new hub.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(
        typeof(ApiResponse<HubResponseDto>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateHubRequestDto request)
    {
        // Get logged-in user's ID from JWT token.
        int? createdBy = GetCurrentUserId();

        var result = await _service.CreateAsync(request, createdBy);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.HubId },
            ApiResponse<HubResponseDto>.Ok(
                result,
                "Hub created successfully."
            )
        );
    }

    // Update an existing hub.
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(
        typeof(ApiResponse<HubResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateHubRequestDto request)
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
                ApiResponse<HubResponseDto>.Fail(
                    "Hub not found."));
        }

        return Ok(
            ApiResponse<HubResponseDto>.Ok(
                result,
                "Hub updated successfully."
            )
        );
    }

    // Soft delete a hub.
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

        var result = await _service.SoftDeleteAsync(
            id,
            updatedBy);

        if (!result)
        {
            return NotFound(
                ApiResponse<bool>.Fail(
                    "Hub not found."));
        }

        return Ok(
            ApiResponse<bool>.Ok(
                true,
                "Hub deleted successfully."
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
