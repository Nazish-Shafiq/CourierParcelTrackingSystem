namespace CourierParcelTrackingSystem.API.DTOs.ScreenAssignmentDTO;

public class ScreenOptionDto
{
    public int ScreenId { get; set; }
    public string ScreenName { get; set; } = string.Empty;
    public string ScreenKey { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string RoutePath { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsAssignable { get; set; }
}

public class ScreenAssignmentUserDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> RoleNames { get; set; } = new();
}

public class ScreenAssignmentResponseDto
{
    public long PermissionId { get; set; }
    public int ScreenId { get; set; }
    public string ScreenName { get; set; } = string.Empty;
    public string ScreenKey { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string RoutePath { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string AssignedUserName { get; set; } = string.Empty;
    public string AssignedUserFullName { get; set; } = string.Empty;
    public List<string> AssignedUserRoles { get; set; } = new();
    public DateTime AssignedAt { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? RevokedAt { get; set; }
}

public class CreateScreenAssignmentRequestDto
{
    public int ScreenId { get; set; }
    public int UserId { get; set; }
}

public class ReassignScreenRequestDto
{
    public int UserId { get; set; }
}
