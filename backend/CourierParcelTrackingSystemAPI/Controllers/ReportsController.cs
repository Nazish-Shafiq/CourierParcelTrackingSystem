
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.ReportDTO;
using CourierParcelTrackingSystem.API.Services.Interfaces;

namespace CourierParcelTrackingSystem.API.Controllers;

[Route("api/reports")]
[ApiController]
[Authorize(Roles = "Admin,Booking Agent,Dispatcher,Courier")]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _reportsService;

    public ReportsController(IReportsService reportsService)
    {
        _reportsService = reportsService;
    }

    // GET: api/reports/parcel-bookings
    [HttpGet("parcel-bookings")]
    public async Task<ActionResult<ApiResponse<List<ParcelBookingReportDto>>>>
        GetParcelBookingReport([FromQuery] ReportRequestDto request)
    {
        var result = await _reportsService.GetParcelBookingReportAsync(request);

        return Ok(
            ApiResponse<List<ParcelBookingReportDto>>.Ok(
                result,
                "Parcel booking report retrieved successfully."));
    }

    // GET: api/reports/parcel-status
    [HttpGet("parcel-status")]
    public async Task<ActionResult<ApiResponse<List<ParcelStatusReportDto>>>>
        GetParcelStatusReport([FromQuery] ReportRequestDto request)
    {
        var result = await _reportsService.GetParcelStatusReportAsync(request);

        return Ok(
            ApiResponse<List<ParcelStatusReportDto>>.Ok(
                result,
                "Parcel status report retrieved successfully."));
    }

    // GET: api/reports/courier-performance
    [HttpGet("courier-performance")]
    public async Task<ActionResult<ApiResponse<List<CourierPerformanceReportDto>>>>
        GetCourierPerformanceReport([FromQuery] ReportRequestDto request)
    {
        var result = await _reportsService.GetCourierPerformanceReportAsync(request);

        return Ok(
            ApiResponse<List<CourierPerformanceReportDto>>.Ok(
                result,
                "Courier performance report retrieved successfully."));
    }
}
