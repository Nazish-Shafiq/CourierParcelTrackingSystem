
using CourierParcelTrackingSystem.API.DTOs.ReportDTO;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IReportsRepository
{
    Task<List<ParcelBookingReportDto>> GetParcelBookingReportAsync(
        ReportRequestDto request);

    Task<List<ParcelStatusReportDto>> GetParcelStatusReportAsync(
        ReportRequestDto request);

    Task<List<CourierPerformanceReportDto>> GetCourierPerformanceReportAsync(
        ReportRequestDto request);
}
