using System;
using System.Collections.Generic;

namespace CourierParcelTrackingSystem.API.Entities;

public partial class AiPrompt
{
    public int AiPromptId { get; set; }

    public string PromptKey { get; set; } = null!;

    public string PromptName { get; set; } = null!;

    public string? Description { get; set; }

    public string ModuleName { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<AiRequest> AiRequests { get; set; } = new List<AiRequest>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
