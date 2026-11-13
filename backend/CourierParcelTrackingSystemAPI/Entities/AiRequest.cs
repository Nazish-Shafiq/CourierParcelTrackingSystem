using System;
using System.Collections.Generic;

namespace CourierParcelTrackingSystem.API.Entities;

public partial class AiRequest
{
    public long AiRequestId { get; set; }

    public int AiPromptId { get; set; }

    public int AiModelId { get; set; }

    public int? RequestedBy { get; set; }

    public long? ParcelId { get; set; }

    public string RequestPurpose { get; set; } = null!;

    public string InputSnapshot { get; set; } = null!;

    public string? OutputSnapshot { get; set; }

    public string RequestStatus { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public DateTime RequestedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual AiModel AiModel { get; set; } = null!;

    public virtual AiPrompt AiPrompt { get; set; } = null!;

    public virtual Parcel? Parcel { get; set; }

    public virtual User? RequestedByNavigation { get; set; }
}
