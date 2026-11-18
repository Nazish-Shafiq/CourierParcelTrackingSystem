
using System.Security.Claims;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelBookingDTO;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierParcelTrackingSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Booking Agent,Dispatcher,Courier")]
public class ParcelBookingController : ControllerBase
{
    private readonly IParcelBookingService _service;

    public ParcelBookingController(IParcelBookingService service)
    {
        _service = service;
    }

    // GET: api/ParcelBooking?page=1&pageSize=10&search=
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationRequestDto pagination,
        [FromQuery] string? search)
    {
        var result = await _service.GetPagedAsync(pagination, search);

        return Ok(
            ApiResponse<PagedResponseDto<ParcelBookingResponseDto>>.Ok(
                result,
                "Parcel bookings retrieved successfully."));
    }

    // GET: api/ParcelBooking/1
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
        {
            return NotFound(
                ApiResponse<ParcelBookingResponseDto>.Fail(
                    "Parcel booking not found."));
        }

        return Ok(
            ApiResponse<ParcelBookingResponseDto>.Ok(
                result,
                "Parcel booking retrieved successfully."));
    }

    // POST: api/ParcelBooking
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateParcelBookingRequestDto request)
    {
        int? createdBy = GetCurrentUserId();

        var result = await _service.CreateBookingAsync(request, createdBy);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<ParcelBookingResponseDto>.Ok(
                result,
                "Parcel booked successfully."));
    }

    // Get logged-in user ID from JWT.
    private int? GetCurrentUserId()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userId, out int id) ? id : null;
    }
}
