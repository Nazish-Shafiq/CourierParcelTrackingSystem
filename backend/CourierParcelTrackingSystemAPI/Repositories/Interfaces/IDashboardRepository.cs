using CourierParcelTrackingSystem.API.DTOs.DashboardDTO;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IDashboardRepository
{
    Task<AdminDashboardResponseDto> GetAdminDashboardAsync();

    Task<DispatcherDashboardResponseDto> GetDispatcherDashboardAsync();

    Task<CourierDashboardResponseDto?> GetCourierDashboardAsync(int userId);
}
