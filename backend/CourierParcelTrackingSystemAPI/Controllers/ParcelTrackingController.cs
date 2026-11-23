
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelTracking;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using CourierParcelTrackingSystem.API.Authorization;

namespace CourierParcelTrackingSystem.API.Controllers;

[Route("api/parcel-tracking")]
[ApiController]
[Authorize]
public class ParcelTrackingController : ControllerBase
{
    private readonly IParcelTrackingService _service;

    public ParcelTrackingController(IParcelTrackingService service)
    {
        _service = service;
    }

    // Get logged-in user's ID from JWT token.
    private int? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userId, out var id) ? id : null;
    }

    private int? GetCourierUserId()
    {
        var assignmentRoutePaths =
            HttpContext.Items[ScreenAccessAttribute.AssignmentRoutePathItemKey]
                as IEnumerable<string> ?? Array.Empty<string>();
        var hasCourierScreenAssignment = assignmentRoutePaths.Any(routePath =>
            routePath.StartsWith("/courier/", StringComparison.OrdinalIgnoreCase));

        return User.IsInRole("Courier") || hasCourierScreenAssignment
            ? GetCurrentUserId()
            : null;
    }

    // GET: api/parcel-tracking
    // Get all tracking events with pagination.
    [HttpGet]
    [ScreenAccess(
        "/booking/track-parcel",
        "/dispatch/tracking",
        "/courier/assigned-parcels",
        "/courier/delivery-history")]
    public async Task<ActionResult<ApiResponse<PagedResponseDto<ParcelTrackingResponseDto>>>> GetAll(
        [FromQuery] PaginationRequestDto request)
    {
        var courierUserId = GetCourierUserId();
        if (User.IsInRole("Courier") && !courierUserId.HasValue)
        {
            return Unauthorized(ApiResponse<PagedResponseDto<ParcelTrackingResponseDto>>.Fail(
                "Invalid user information in token."));
        }

        var result = await _service.GetPagedAsync(request, courierUserId);

        return Ok(ApiResponse<PagedResponseDto<ParcelTrackingResponseDto>>.Ok(
            result,
            "Tracking events retrieved successfully."));
    }

    // GET: api/parcel-tracking/{trackingNumber}
    // Get complete tracking history by tracking number.
    [HttpGet("{trackingNumber}")]
    [ScreenAccess(
        "/booking/track-parcel",
        "/dispatch/tracking",
        "/courier/assigned-parcels",
        "/courier/delivery-history")]
    public async Task<ActionResult<ApiResponse<List<ParcelTrackingResponseDto>>>> GetTrackingHistory(
        string trackingNumber)
    {
        var courierUserId = GetCourierUserId();
        if (User.IsInRole("Courier") && !courierUserId.HasValue)
        {
            return Unauthorized(ApiResponse<List<ParcelTrackingResponseDto>>.Fail(
                "Invalid user information in token."));
        }

        var result = await _service.GetTrackingHistoryAsync(trackingNumber, courierUserId);

        return Ok(ApiResponse<List<ParcelTrackingResponseDto>>.Ok(
            result,
            "Parcel tracking history retrieved successfully."));
    }

    // POST: api/parcel-tracking
    // Create a new tracking event.
    [HttpPost]
    [ScreenAccess("/booking/bookings", "/dispatch/tracking")]
    public async Task<ActionResult<ApiResponse<ParcelTrackingResponseDto>>> CreateStatusEvent(
        [FromBody] CreateStatusEventRequestDto request)
    {
        var result = await _service.CreateStatusEventAsync(
            request,
            GetCurrentUserId());

        return Ok(ApiResponse<ParcelTrackingResponseDto>.Ok(
            result,
            "Parcel tracking event created successfully."));
    }

    // PUT: api/parcel-tracking/{parcelId}/status
    // Update parcel status and create a tracking event.
    [HttpPut("{parcelId:long}/status")]
    [ScreenAccess("/booking/bookings", "/dispatch/tracking")]
    public async Task<ActionResult<ApiResponse<ParcelTrackingResponseDto>>> UpdateParcelStatus(
        long parcelId,
        [FromBody] UpdateParcelStatusRequestDto request)
    {
        var result = await _service.UpdateParcelStatusAsync(
            parcelId,
            request,
            GetCurrentUserId());

        return Ok(ApiResponse<ParcelTrackingResponseDto>.Ok(
            result,
            "Parcel status updated successfully."));
    }

    // PUT: api/parcel-tracking/{parcelId}/courier-transition
    [HttpPut("{parcelId:long}/courier-transition")]
    [ScreenAccess("/courier/assigned-parcels", AllowAdmin = false)]
    public async Task<ActionResult<ApiResponse<ParcelTrackingResponseDto>>> UpdateCourierParcelStatus(
        long parcelId,
        [FromBody] UpdateCourierParcelStatusRequestDto request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(
                ApiResponse<ParcelTrackingResponseDto>.Fail(
                    "Invalid user information in token."));
        }

        try
        {
            var result = await _service.UpdateCourierParcelStatusAsync(
                parcelId,
                request,
                userId.Value);

            return Ok(ApiResponse<ParcelTrackingResponseDto>.Ok(
                result,
                "Courier parcel status updated successfully."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse<ParcelTrackingResponseDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                ApiResponse<ParcelTrackingResponseDto>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                ApiResponse<ParcelTrackingResponseDto>.Fail(ex.Message));
        }
    }
}
