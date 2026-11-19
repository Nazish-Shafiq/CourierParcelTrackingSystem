using System.Security.Claims;
using CourierParcelTrackingSystem.API.DTOs.AuthenticationDTO;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CourierParcelTrackingSystem.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;
    public AuthController(IAuthService service) => _service = service;

    [HttpPost("bootstrap-admin")]
    [AllowAnonymous]
    public async Task<IActionResult> BootstrapAdmin(BootstrapAdminRequestDto request)
    {
        try { return Ok(ApiResponse<string>.Ok(await _service.BootstrapAdminAsync(request))); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(ApiResponse<string>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<string>.Fail(ex.Message)); }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        var result = await _service.LoginAsync(request);
        if (result == null) return Unauthorized(ApiResponse<LoginResponseDto>.Fail("Invalid username/email or password, or account has no active role."));
        return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Login successful."));
    }

    [HttpPost("register-staff")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RegisterStaff(RegisterStaffRequestDto request)
    {
        try { return StatusCode(201, ApiResponse<string>.Ok(await _service.RegisterStaffAsync(request, CurrentUserId()), "Staff account created.")); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<string>.Fail(ex.Message)); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
    }

    [HttpGet("staff-roles")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStaffRoles()
    {
        var roles = await _service.GetStaffRolesAsync();
        return Ok(ApiResponse<IReadOnlyList<StaffRoleOptionDto>>.Ok(
            roles,
            "Staff roles retrieved successfully."));
    }

    [HttpGet("roles")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAssignableRoles()
    {
        var roles = await _service.GetAssignableRolesAsync();
        return Ok(ApiResponse<IReadOnlyList<RoleOptionDto>>.Ok(
            roles,
            "Active roles retrieved successfully."));
    }

    [HttpPost("register-customer")]
    [Authorize(Roles = "Booking Agent")]
    public async Task<IActionResult> RegisterCustomer(RegisterCustomerRequestDto request)
    {
        try { return StatusCode(201, ApiResponse<string>.Ok(await _service.RegisterCustomerAsync(request, CurrentUserId()), "Customer account created.")); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<string>.Fail(ex.Message)); }
    }


    // JWT logout is client-side: the client must discard its stored access token.
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return Ok(ApiResponse<string>.Ok(
            "Logout successful. Remove the access token from the client.",
            "Logout successful."));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var result = await _service.GetCurrentUserAsync(CurrentUserId());
        return result == null
            ? NotFound(ApiResponse<LoginResponseDto>.Fail("User account not found or inactive."))
            : Ok(ApiResponse<object>.Ok(new { result.UserId, result.UserName, result.FullName, result.Email, result.Roles, result.RoleIds }));
    }

    private int CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(value, out var id)) throw new UnauthorizedAccessException();
        return id;
    }
}
