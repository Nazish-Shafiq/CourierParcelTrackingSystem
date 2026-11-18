using System.Security.Claims;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.CustomerDashboardDTO;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierParcelTrackingSystem.API.Controllers;

[ApiController]
[Authorize(Roles = "Customer Viewer")]
[Route("api/customer-dashboard")]
public class CustomerDashboardController : ControllerBase
{
    private readonly ICustomerDashboardService _service;

    public CustomerDashboardController(ICustomerDashboardService service)
    {
        _service = service;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<CustomerDashboardSummaryDto>>> GetStats()
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized(ApiResponse<CustomerDashboardSummaryDto>.Fail(
                "Invalid user information in token."));

        var result = await _service.GetSummaryAsync(userId);
        return Ok(ApiResponse<CustomerDashboardSummaryDto>.Ok(
            result,
            "Customer dashboard statistics retrieved successfully."));
    }

    [HttpGet("parcels")]
    public async Task<ActionResult<ApiResponse<PagedResponseDto<CustomerParcelDto>>>> GetParcels(
        [FromQuery] PaginationRequestDto pagination,
        [FromQuery] string? search,
        [FromQuery] string? statusCode)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized(ApiResponse<PagedResponseDto<CustomerParcelDto>>.Fail(
                "Invalid user information in token."));

        var result = await _service.GetParcelsAsync(
            userId,
            pagination,
            search,
            statusCode);

        return Ok(ApiResponse<PagedResponseDto<CustomerParcelDto>>.Ok(
            result,
            "Customer parcels retrieved successfully."));
    }

    [HttpGet("parcels/{parcelId:long}")]
    public async Task<ActionResult<ApiResponse<CustomerParcelDetailsDto>>> GetParcelDetails(
        long parcelId)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized(ApiResponse<CustomerParcelDetailsDto>.Fail(
                "Invalid user information in token."));

        var result = await _service.GetParcelDetailsAsync(userId, parcelId);
        return result is null
            ? NotFound(ApiResponse<CustomerParcelDetailsDto>.Fail("Parcel not found."))
            : Ok(ApiResponse<CustomerParcelDetailsDto>.Ok(
                result,
                "Customer parcel details retrieved successfully."));
    }

    private bool TryGetCurrentUserId(out int userId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
