using System.Security.Claims;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ScreenAssignmentDTO;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierParcelTrackingSystem.API.Controllers;

[ApiController]
[Route("api/screen-assignments")]
[Authorize]
public class ScreenAssignmentsController : ControllerBase
{
    private readonly IScreenAssignmentService _service;

    public ScreenAssignmentsController(IScreenAssignmentService service) =>
        _service = service;

    [HttpGet("mine")]
    public async Task<ActionResult<ApiResponse<List<ScreenOptionDto>>>> GetMyScreens()
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized(ApiResponse<List<ScreenOptionDto>>.Fail(
                "Invalid user information in token."));

        return Ok(ApiResponse<List<ScreenOptionDto>>.Ok(
            await _service.GetMyScreensAsync(userId)));
    }

    [HttpGet("screens")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<ScreenOptionDto>>>> GetScreens() =>
        Ok(ApiResponse<List<ScreenOptionDto>>.Ok(
            await _service.GetActiveScreensAsync()));

    [HttpGet("eligible-users")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<ScreenAssignmentUserDto>>>> GetEligibleUsers(
        [FromQuery] int screenId) =>
        Ok(ApiResponse<List<ScreenAssignmentUserDto>>.Ok(
            await _service.GetEligibleUsersAsync(screenId)));

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<ScreenAssignmentResponseDto>>>> GetAssignments() =>
        Ok(ApiResponse<List<ScreenAssignmentResponseDto>>.Ok(
            await _service.GetAssignmentsAsync()));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ScreenAssignmentResponseDto>>> Assign(
        [FromBody] CreateScreenAssignmentRequestDto request)
    {
        if (!TryGetCurrentUserId(out var adminUserId))
            return Unauthorized(ApiResponse<ScreenAssignmentResponseDto>.Fail(
                "Invalid user information in token."));

        try
        {
            var result = await _service.AssignAsync(request, adminUserId);
            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<ScreenAssignmentResponseDto>.Ok(
                    result,
                    "Screen assigned successfully."));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(ApiResponse<ScreenAssignmentResponseDto>.Fail(
                exception.Message));
        }
    }

    [HttpPut("{permissionId:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ScreenAssignmentResponseDto>>> Reassign(
        long permissionId,
        [FromBody] ReassignScreenRequestDto request)
    {
        if (!TryGetCurrentUserId(out var adminUserId))
            return Unauthorized(ApiResponse<ScreenAssignmentResponseDto>.Fail(
                "Invalid user information in token."));

        try
        {
            var result = await _service.ReassignAsync(
                permissionId,
                request,
                adminUserId);
            return result is null
                ? NotFound(ApiResponse<ScreenAssignmentResponseDto>.Fail(
                    "Active screen assignment not found."))
                : Ok(ApiResponse<ScreenAssignmentResponseDto>.Ok(
                    result,
                    "Screen responsibility reassigned successfully."));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(ApiResponse<ScreenAssignmentResponseDto>.Fail(
                exception.Message));
        }
    }

    [HttpDelete("{permissionId:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> Revoke(long permissionId)
    {
        if (!TryGetCurrentUserId(out var adminUserId))
            return Unauthorized(ApiResponse<bool>.Fail(
                "Invalid user information in token."));

        var revoked = await _service.RevokeAsync(permissionId, adminUserId);
        return revoked
            ? Ok(ApiResponse<bool>.Ok(true, "Screen assignment revoked successfully."))
            : NotFound(ApiResponse<bool>.Fail("Active screen assignment not found."));
    }

    private bool TryGetCurrentUserId(out int userId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
