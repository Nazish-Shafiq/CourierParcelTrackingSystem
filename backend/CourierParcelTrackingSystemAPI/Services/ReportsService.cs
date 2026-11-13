
using CourierParcelTrackingSystem.API.DTOs.ReportDTO;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;

namespace CourierParcelTrackingSystem.API.Services.Implementations;

public class ReportsService : IReportsService
{
    private readonly IReportsRepository _reportsRepository;

    public ReportsService(IReportsRepository reportsRepository)
    {
        _reportsRepository = reportsRepository;
    }

    public async Task<List<ParcelBookingReportDto>> GetParcelBookingReportAsync(
        ReportRequestDto request)
    {
        ValidateDateRange(request);

        return await _reportsRepository.GetParcelBookingReportAsync(request);
    }

    public async Task<List<ParcelStatusReportDto>> GetParcelStatusReportAsync(
        ReportRequestDto request)
    {
        ValidateDateRange(request);

        return await _reportsRepository.GetParcelStatusReportAsync(request);
    }

    public async Task<List<CourierPerformanceReportDto>>
        GetCourierPerformanceReportAsync(ReportRequestDto request)
    {
        ValidateDateRange(request);

        return await _reportsRepository.GetCourierPerformanceReportAsync(request);
    }

    private static void ValidateDateRange(ReportRequestDto request)
    {
        if (request.FromDate.HasValue &&
            request.ToDate.HasValue &&
            request.FromDate.Value.Date > request.ToDate.Value.Date)
        {
            throw new ArgumentException(
                "FromDate cannot be later than ToDate.");
        }
    }
}
