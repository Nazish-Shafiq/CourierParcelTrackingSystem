using CourierParcelTrackingSystem.API.DTOs.AuditLogDTO;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierParcelTrackingSystem.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/audit-logs")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogsController(IAuditLogService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResponseDto<AuditLogResponseDto>>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponseDto<AuditLogResponseDto>>>> Get(
        [FromQuery] AuditLogQueryDto query)
    {
        try
        {
            var result = await _service.GetPagedAsync(query);
            return Ok(ApiResponse<PagedResponseDto<AuditLogResponseDto>>.Ok(
                result,
                "Audit logs retrieved successfully."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                ApiResponse<PagedResponseDto<AuditLogResponseDto>>.Fail(
                    ex.Message));
        }
    }
}
