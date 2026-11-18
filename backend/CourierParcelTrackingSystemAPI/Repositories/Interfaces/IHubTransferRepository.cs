using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IHubTransferRepository
{
    Task<PagedResponseDto<HubTransfer>> GetPagedAsync(PaginationRequestDto request, string? search, string? status);
    Task<HubTransfer?> GetByIdAsync(long transferId);
    Task<Parcel?> GetParcelForTransferAsync(long parcelId);
    Task<Hub?> GetActiveHubAsync(int hubId);
    Task<bool> HasActiveTransferAsync(long parcelId);
    Task<ParcelStatus?> GetStatusByCodeAsync(string statusCode);
    Task<List<Parcel>> GetEligibleParcelsAsync();
    Task<List<Hub>> GetActiveHubsAsync();
    Task AddStatusEventAsync(StatusEvent statusEvent);
    Task AddAuditLogAsync(AuditLog auditLog);
    Task AddAsync(HubTransfer transfer);
    Task SaveChangesAsync();
}
