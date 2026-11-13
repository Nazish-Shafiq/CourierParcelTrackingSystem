using CourierParcelTrackingSystem.API.DTOs.DashboardDTO;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface IDashboardService
{
    Task<AdminDashboardResponseDto> GetAdminDashboardAsync();

    Task<DispatcherDashboardResponseDto> GetDispatcherDashboardAsync();

    Task<CourierDashboardResponseDto> GetCourierDashboardAsync(int userId);
}
