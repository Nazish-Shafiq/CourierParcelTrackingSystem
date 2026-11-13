
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IParcelStatusRepository
{
    Task<PagedResponseDto<ParcelStatus>> GetPagedAsync(
        PaginationRequestDto pagination,
        string? search);

    Task<ParcelStatus?> GetByIdAsync(int id);

    Task<bool> StatusNameExistsAsync(
        string statusName,
        int? excludeStatusId = null);

    Task<bool> StatusCodeExistsAsync(string statusCode);

    Task AddAsync(ParcelStatus parcelStatus);

    Task UpdateAsync(ParcelStatus parcelStatus);

    Task SoftDeleteAsync(ParcelStatus parcelStatus);
}
