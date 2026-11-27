namespace CourierParcelTrackingSystem.API.DTOs.AuditLogDTO;

public class AuditLogResponseDto
{
    public long AuditLogId { get; set; }
    public int? ActorUserId { get; set; }
    public string? ActorFullName { get; set; }
    public string? ActorUserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public DateTime OccurredAt { get; set; }
}
