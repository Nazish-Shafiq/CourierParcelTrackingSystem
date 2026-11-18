namespace CourierParcelTrackingSystem.API.Entities;

public partial class HubTransfer
{
    public long HubTransferId { get; set; }
    public long ParcelId { get; set; }
    public int SourceHubId { get; set; }
    public int DestinationHubId { get; set; }
    public string Status { get; set; } = "IN_TRANSIT";
    public DateTime TransferredAt { get; set; }
    public DateTime? ArrivedAt { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }

    public virtual Parcel Parcel { get; set; } = null!;
    public virtual Hub SourceHub { get; set; } = null!;
    public virtual Hub DestinationHub { get; set; } = null!;
    public virtual User? CreatedByNavigation { get; set; }
    public virtual User? UpdatedByNavigation { get; set; }
}
