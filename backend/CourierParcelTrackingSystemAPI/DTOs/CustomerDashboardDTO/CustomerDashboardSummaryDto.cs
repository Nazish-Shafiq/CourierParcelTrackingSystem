namespace CourierParcelTrackingSystem.API.DTOs.CustomerDashboardDTO;

public class CustomerDashboardSummaryDto
{
    public int TotalParcels { get; set; }
    public int BookedParcels { get; set; }
    public int DeliveredParcels { get; set; }
    public List<CustomerParcelStatusOptionDto> AvailableStatuses { get; set; } = new();
}

public class CustomerParcelStatusOptionDto
{
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
}
