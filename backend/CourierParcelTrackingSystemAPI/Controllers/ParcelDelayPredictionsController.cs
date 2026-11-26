using System.Security.Claims;
using CourierParcelTrackingSystem.API.Authorization;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelDelayPredictionDTO;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierParcelTrackingSystem.API.Controllers;

[ApiController]
[Route("api/parcel-delay-predictions")]
[Authorize]
public class ParcelDelayPredictionsController : ControllerBase
{
    private readonly IParcelDelayPredictionService _service;

    public ParcelDelayPredictionsController(
        IParcelDelayPredictionService service) =>
        _service = service;

    [HttpGet("dispatcher")]
    [Authorize(Roles = "Dispatcher")]
    [ScreenAccess("/dashboard/dispatcher", AllowAdmin = false)]
    public async Task<ActionResult<ApiResponse<ParcelDelayPredictionsResponseDto>>>
        GetDispatcherPredictions()
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized(ApiResponse<ParcelDelayPredictionsResponseDto>.Fail(
                "Invalid user information in token."));

        return Ok(ApiResponse<ParcelDelayPredictionsResponseDto>.Ok(
            await _service.GetDispatcherPredictionsAsync(userId)));
    }

    [HttpGet("admin/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ParcelDelayModelStatusDto>>>
        GetAdminStatus() =>
        Ok(ApiResponse<ParcelDelayModelStatusDto>.Ok(
            await _service.GetAdminStatusAsync()));

    [HttpPost("admin/train")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ParcelDelayModelStatusDto>>>
        TrainModel()
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized(ApiResponse<ParcelDelayModelStatusDto>.Fail(
                "Invalid user information in token."));

        return Ok(ApiResponse<ParcelDelayModelStatusDto>.Ok(
            await _service.TrainAsync(userId),
            "Parcel delay model training readiness and evaluation completed."));
    }

    private bool TryGetCurrentUserId(out int userId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
