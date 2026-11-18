
using System.Security.Claims;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.CustomerDTO;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierParcelTrackingSystem.API.Controllers;

[Authorize(Roles = "Admin,Booking Agent")]
[ApiController]
[Route("api/customers")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _service;

    public CustomerController(ICustomerService service)
    {
        _service = service;
    }

    // Get all customers with pagination and optional search.
    [HttpGet]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] PaginationRequestDto pagination,
        [FromQuery] string? search)
    {
        var result = await _service.GetPagedAsync(pagination, search);

        return Ok(
            ApiResponse<PagedResponseDto<CustomerResponseDto>>.Ok(
                result,
                "Customers retrieved successfully."
            )
        );
    }

    // Get customer by ID.
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<CustomerResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        return result is null
            ? NotFound(
                ApiResponse<CustomerResponseDto>.Fail(
                    "Customer not found."))
            : Ok(
                ApiResponse<CustomerResponseDto>.Ok(result));
    }

    // Create a new customer.
    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponse<CustomerResponseDto>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerRequestDto request)
    {
        // Get logged-in user's ID from JWT token.
        int? createdBy = GetCurrentUserId();

        var result = await _service.CreateAsync(request, createdBy);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.CustomerId },
            ApiResponse<CustomerResponseDto>.Ok(
                result,
                "Customer created successfully."
            )
        );
    }

    // Update an existing customer.
    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<CustomerResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCustomerRequestDto request)
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
                ApiResponse<CustomerResponseDto>.Fail(
                    "Customer not found."));
        }

        return Ok(
            ApiResponse<CustomerResponseDto>.Ok(
                result,
                "Customer updated successfully."
            )
        );
    }

    // Soft delete a customer.
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
                    "Customer not found."));
        }

        return Ok(
            ApiResponse<bool>.Ok(
                true,
                "Customer deleted successfully."
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
