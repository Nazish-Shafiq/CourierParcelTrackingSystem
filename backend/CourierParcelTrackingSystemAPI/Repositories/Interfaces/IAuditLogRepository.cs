using CourierParcelTrackingSystem.API.DTOs.AuditLogDTO;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories.Interfaces;

public interface IAuditLogRepository
{
    Task<PagedResponseDto<AuditLog>> GetPagedAsync(AuditLogQueryDto query);
    Task AddAsync(AuditLog auditLog);
}
