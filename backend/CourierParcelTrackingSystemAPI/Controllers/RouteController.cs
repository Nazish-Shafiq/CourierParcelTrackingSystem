
using System.Security.Claims;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.RouteDTO;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using CourierParcelTrackingSystem.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierParcelTrackingSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RouteController : ControllerBase
{
    private readonly IRouteService _routeService;

    public RouteController(IRouteService routeService)
    {
        _routeService = routeService;
    }

    // GET: api/Route?page=1&pageSize=10&search=
    [HttpGet]
    [ScreenAccess("/dispatch/routes", "/booking/book-parcel", "/dispatch/assign")]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationRequestDto pagination,
        [FromQuery] string? search,
        [FromQuery] bool? isActive)
    {
        var result = await _routeService.GetPagedAsync(
            pagination, search, isActive);

        return Ok(
            ApiResponse<PagedResponseDto<RouteResponseDto>>.Ok(
                result,
                "Routes retrieved successfully."));
    }

    // GET: api/Route/1
    [HttpGet("{id:int}")]
    [ScreenAccess("/dispatch/routes", "/booking/book-parcel", "/dispatch/assign")]
    public async Task<IActionResult> GetById(int id)
    {
        var route = await _routeService.GetByIdAsync(id);

        if (route == null)
        {
            return NotFound(
                ApiResponse<RouteResponseDto>.Fail(
                    "Route not found."));
        }

        return Ok(
            ApiResponse<RouteResponseDto>.Ok(
                route,
                "Route retrieved successfully."));
    }

    // POST: api/Route
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        [FromBody] CreateRouteRequestDto request)
    {
        int? createdBy = GetCurrentUserId();

        var route = await _routeService.CreateAsync(
            request, createdBy);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<RouteResponseDto>.Ok(
                route,
                "Route created successfully."));
    }

    // PUT: api/Route/1
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateRouteRequestDto request)
    {
        int? updatedBy = GetCurrentUserId();

        var route = await _routeService.UpdateAsync(
            id, request, updatedBy);

        if (route == null)
        {
            return NotFound(
                ApiResponse<RouteResponseDto>.Fail(
                    "Route not found."));
        }

        return Ok(
            ApiResponse<RouteResponseDto>.Ok(
                route,
                "Route updated successfully."));
    }

    // DELETE: api/Route/1
    // Soft delete only; the database record is retained.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        int? updatedBy = GetCurrentUserId();

        bool deleted = await _routeService.SoftDeleteAsync(
            id, updatedBy);

        if (!deleted)
        {
            return NotFound(
                ApiResponse<bool>.Fail(
                    "Route not found."));
        }

        return Ok(
            ApiResponse<bool>.Ok(
                true,
                "Route deleted successfully."));
    }

    // Read the logged-in user's ID from the JWT token.
    private int? GetCurrentUserId()
    {
        string? userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(userId, out int id)
            ? id
            : null;
    }
}
