using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.UserDTO;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CourierParcelTrackingSystem.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;
    public UserController(IUserService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<UserResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() =>
        Ok(ApiResponse<List<UserResponseDto>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<UserResponseDto>.Fail("User not found."))
            : Ok(ApiResponse<UserResponseDto>.Ok(result));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateUserRequestDto request)
    {
        try
        {
            var result = await _service.UpdateAsync(
                id,
                request,
                GetCurrentUserId());
            return result is null
                ? NotFound(ApiResponse<UserResponseDto>.Fail("User not found."))
                : Ok(ApiResponse<UserResponseDto>.Ok(
                    result,
                    "User updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<UserResponseDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _service.SoftDeleteAsync(
                id,
                GetCurrentUserId());
            return !deleted
                ? NotFound(ApiResponse<bool>.Fail("User not found."))
                : Ok(ApiResponse<bool>.Ok(true, "User deactivated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<bool>.Fail(ex.Message));
        }
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(value, out var userId))
            throw new UnauthorizedAccessException("Invalid user information in token.");
        return userId;
    }
}
