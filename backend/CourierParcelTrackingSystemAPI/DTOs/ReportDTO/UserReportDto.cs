namespace CourierParcelTrackingSystem.API.DTOs.ReportDTO;

public class UserReportDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public List<UserRoleReportDto> Roles { get; set; } = new();
}

public class UserRoleReportDto
{
    public string RoleName { get; set; } = string.Empty;
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
}
