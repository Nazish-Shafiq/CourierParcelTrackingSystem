using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.DTOs.CustomerDashboardDTO;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface ICustomerDashboardService
{
    Task<CustomerDashboardSummaryDto> GetSummaryAsync(int userId);

    Task<PagedResponseDto<CustomerParcelDto>> GetParcelsAsync(
        int userId,
        PaginationRequestDto pagination,
        string? search,
        string? statusCode);

    Task<CustomerParcelDetailsDto?> GetParcelDetailsAsync(int userId, long parcelId);

    Task<CustomerParcelDetailsDto?> GetParcelByTrackingNumberAsync(
        int userId,
        string trackingNumber);
}
