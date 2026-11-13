
using CourierParcelTrackingSystem.API.DTOs.ReportDTO;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface IReportsService
{
    // Retrieves parcel booking details within the specified date range.
    Task<List<ParcelBookingReportDto>> GetParcelBookingReportAsync(
        ReportRequestDto request);

    // Retrieves parcel counts grouped by their current status.
    Task<List<ParcelStatusReportDto>> GetParcelStatusReportAsync(
        ReportRequestDto request);

    // Retrieves courier assignment and delivery performance details.
    Task<List<CourierPerformanceReportDto>> GetCourierPerformanceReportAsync(
        ReportRequestDto request);
}
