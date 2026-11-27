using CourierParcelTrackingSystem.API.DTOs.Common;

namespace CourierParcelTrackingSystem.API.DTOs.AuditLogDTO;

public class AuditLogQueryDto : PaginationRequestDto
{
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? Action { get; set; }
    public int? ActorUserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool SortAscending { get; set; }
}
