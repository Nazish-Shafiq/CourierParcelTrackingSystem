
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.DashboardDTO;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CourierParcelTrackingSystem.API.Controllers;

[Route("api/dashboard")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // GET: api/dashboard/admin
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<AdminDashboardResponseDto>>> GetAdminDashboard()
    {
        var result = await _dashboardService.GetAdminDashboardAsync();

        return Ok(ApiResponse<AdminDashboardResponseDto>.Ok(
            result,
            "Admin dashboard data retrieved successfully."
        ));
    }

    // GET: api/dashboard/dispatcher
    [HttpGet("dispatcher")]
    [Authorize(Roles = "Dispatcher")]
    public async Task<ActionResult<ApiResponse<DispatcherDashboardResponseDto>>> GetDispatcherDashboard()
    {
        var result = await _dashboardService.GetDispatcherDashboardAsync();

        return Ok(ApiResponse<DispatcherDashboardResponseDto>.Ok(
            result,
            "Dispatcher dashboard data retrieved successfully."));
    }

    // GET: api/dashboard/courier
    [HttpGet("courier")]
    [Authorize(Roles = "Courier")]
    public async Task<ActionResult<ApiResponse<CourierDashboardResponseDto>>> GetCourierDashboard()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(
                ApiResponse<CourierDashboardResponseDto>.Fail(
                    "Invalid user information in token."));
        }

        var result = await _dashboardService.GetCourierDashboardAsync(userId);

        return Ok(ApiResponse<CourierDashboardResponseDto>.Ok(
            result,
            "Courier dashboard data retrieved successfully."));
    }
}
