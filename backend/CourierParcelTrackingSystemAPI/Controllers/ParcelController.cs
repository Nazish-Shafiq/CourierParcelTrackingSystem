using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ParcelDTO;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierParcelTrackingSystem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/parcels")]
public class ParcelController : ControllerBase
{
    private readonly IParcelService _service;
    public ParcelController(IParcelService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ParcelResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() =>
        Ok(ApiResponse<List<ParcelResponseDto>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<ParcelResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<ParcelResponseDto>.Fail("Parcel not found."))
            : Ok(ApiResponse<ParcelResponseDto>.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ParcelResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateParcelRequestDto request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.ParcelId },
            ApiResponse<ParcelResponseDto>.Ok(result, "Parcel created successfully."));
    }
}
