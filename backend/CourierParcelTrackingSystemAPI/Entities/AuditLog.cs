using System;
using System.Collections.Generic;

namespace CourierParcelTrackingSystem.API.Entities;

public partial class AuditLog
{
    public long AuditLogId { get; set; }

    public int? ActorUserId { get; set; }

    public string Action { get; set; } = null!;

    public string EntityName { get; set; } = null!;

    public string EntityId { get; set; } = null!;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }

    public DateTime OccurredAt { get; set; }

    public virtual User? ActorUser { get; set; }
}
