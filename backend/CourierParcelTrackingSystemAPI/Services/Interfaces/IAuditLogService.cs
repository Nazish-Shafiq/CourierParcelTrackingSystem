using CourierParcelTrackingSystem.API.DTOs.AuditLogDTO;
using CourierParcelTrackingSystem.API.DTOs.Common;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface IAuditLogService
{
    Task<PagedResponseDto<AuditLogResponseDto>> GetPagedAsync(
        AuditLogQueryDto query);

    Task RecordAsync(
        int? actorUserId,
        string action,
        string entityName,
        string entityId,
        IReadOnlyDictionary<string, object?>? oldValues = null,
        IReadOnlyDictionary<string, object?>? newValues = null);

    Task RecordAuthenticationEventAsync(string action, int? actorUserId);
}
