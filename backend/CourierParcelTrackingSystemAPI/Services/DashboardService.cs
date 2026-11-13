
using CourierParcelTrackingSystem.API.DTOs.DashboardDTO;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services.Interfaces;

namespace CourierParcelTrackingSystem.API.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<AdminDashboardResponseDto> GetAdminDashboardAsync()
    {
        return await _dashboardRepository.GetAdminDashboardAsync();
    }

    public async Task<DispatcherDashboardResponseDto> GetDispatcherDashboardAsync()
    {
        return await _dashboardRepository.GetDispatcherDashboardAsync();
    }

    public async Task<CourierDashboardResponseDto> GetCourierDashboardAsync(int userId)
    {
        var result = await _dashboardRepository.GetCourierDashboardAsync(userId);

        if (result == null)
        {
            throw new KeyNotFoundException(
                "No active courier profile was found for the logged-in user.");
        }

        return result;
    }
}
