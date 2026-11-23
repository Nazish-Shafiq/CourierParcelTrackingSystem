using CourierParcelTrackingSystem.API.DTOs.ScreenAssignmentDTO;

namespace CourierParcelTrackingSystem.API.Services.Interfaces;

public interface IScreenAssignmentService
{
    Task<List<ScreenOptionDto>> GetActiveScreensAsync();
    Task<List<ScreenAssignmentUserDto>> GetEligibleUsersAsync(int screenId);
    Task<List<ScreenAssignmentResponseDto>> GetAssignmentsAsync();
    Task<List<ScreenOptionDto>> GetMyScreensAsync(int userId);
    Task<List<string>> GetActiveAssignmentRoutePathsAsync(
        int userId,
        IReadOnlyCollection<string> screenRoutePaths);
    Task<ScreenAssignmentResponseDto> AssignAsync(
        CreateScreenAssignmentRequestDto request,
        int assignedBy);
    Task<ScreenAssignmentResponseDto?> ReassignAsync(
        long permissionId,
        ReassignScreenRequestDto request,
        int assignedBy);
    Task<bool> RevokeAsync(long permissionId, int revokedBy);
}
