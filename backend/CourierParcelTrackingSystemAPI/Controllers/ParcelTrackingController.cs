
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelTracking;
using CourierParcelTrackingSystem.API.Services.Interfaces;

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

    // GET: api/parcel-tracking
    // Get all tracking events with pagination.
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponseDto<ParcelTrackingResponseDto>>>> GetAll(
        [FromQuery] PaginationRequestDto request)
    {
        var result = await _service.GetPagedAsync(request);

        return Ok(ApiResponse<PagedResponseDto<ParcelTrackingResponseDto>>.Ok(
            result,
            "Tracking events retrieved successfully."));
    }

    // GET: api/parcel-tracking/{trackingNumber}
    // Get complete tracking history by tracking number.
    [HttpGet("{trackingNumber}")]
    public async Task<ActionResult<ApiResponse<List<ParcelTrackingResponseDto>>>> GetTrackingHistory(
        string trackingNumber)
    {
        var result = await _service.GetTrackingHistoryAsync(trackingNumber);

        return Ok(ApiResponse<List<ParcelTrackingResponseDto>>.Ok(
            result,
            "Parcel tracking history retrieved successfully."));
    }

    // POST: api/parcel-tracking
    // Create a new tracking event.
    [HttpPost]
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
}
