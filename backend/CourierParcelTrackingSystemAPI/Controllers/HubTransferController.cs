using System.Security.Claims;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.HubTransferDTO;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierParcelTrackingSystem.API.Controllers;

[ApiController]
[Route("api/hub-transfers")]
[Authorize(Roles = "Admin,Dispatcher")]
public class HubTransferController : ControllerBase
{
    private readonly IHubTransferService _service;

    public HubTransferController(IHubTransferService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationRequestDto request,
        [FromQuery] string? search,
        [FromQuery] string? status)
    {
        var result = await _service.GetPagedAsync(request, search, status);
        return Ok(ApiResponse<PagedResponseDto<HubTransferResponseDto>>.Ok(
            result, "Hub transfers retrieved successfully."));
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions()
    {
        var result = await _service.GetOptionsAsync();
        return Ok(ApiResponse<HubTransferOptionsDto>.Ok(
            result, "Hub transfer options retrieved successfully."));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<HubTransferResponseDto>.Fail("Hub transfer not found."))
            : Ok(ApiResponse<HubTransferResponseDto>.Ok(
                result, "Hub transfer retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHubTransferRequestDto request)
    {
        var result = await _service.CreateAsync(request, CurrentUserId());
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<HubTransferResponseDto>.Ok(result, "Hub transfer created successfully."));
    }

    [HttpPut("{id:long}/confirm-arrival")]
    public async Task<IActionResult> ConfirmArrival(
        long id,
        [FromBody] ConfirmHubTransferArrivalRequestDto request)
    {
        var result = await _service.ConfirmArrivalAsync(id, request, CurrentUserId());
        return Ok(ApiResponse<HubTransferResponseDto>.Ok(
            result, "Parcel arrival confirmed successfully."));
    }

    private int CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException();
    }
}
