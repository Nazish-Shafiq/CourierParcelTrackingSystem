using System.Security.Claims;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CourierParcelTrackingSystem.API.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ScreenAccessAttribute : Attribute, IAsyncAuthorizationFilter
{
    public const string AssignmentRoutePathItemKey =
        "ScreenAccess.AssignmentRoutePath";

    private readonly string[] _screenRoutePaths;

    public ScreenAccessAttribute(params string[] screenRoutePaths) =>
        _screenRoutePaths = screenRoutePaths;

    public bool AllowAdmin { get; set; } = true;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if ((AllowAdmin && user.IsInRole("Admin")) || HasRoleAccess(user))
            return;

        if (!int.TryParse(
                user.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            context.Result = new UnauthorizedObjectResult(
                ApiResponse<string>.Fail("Invalid user information in token."));
            return;
        }

        var service = context.HttpContext.RequestServices
            .GetRequiredService<IScreenAssignmentService>();
        var assignmentRoutePaths = await service.GetActiveAssignmentRoutePathsAsync(
            userId,
            _screenRoutePaths);
        if (assignmentRoutePaths.Count == 0)
        {
            context.Result = new ObjectResult(ApiResponse<string>.Fail(
                "You are not assigned to a screen permitted to perform this action."))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        context.HttpContext.Items[AssignmentRoutePathItemKey] = assignmentRoutePaths;
    }

    private bool HasRoleAccess(ClaimsPrincipal user) =>
        _screenRoutePaths
            .Select(GetRequiredRole)
            .Where(role => role is not null)
            .Any(role => user.IsInRole(role!));

    private static string? GetRequiredRole(string routePath)
    {
        if (routePath.Equals(
                "/dashboard/booking-agent",
                StringComparison.OrdinalIgnoreCase) ||
            routePath.StartsWith("/booking/", StringComparison.OrdinalIgnoreCase))
            return "Booking Agent";

        if (routePath.Equals(
                "/dashboard/dispatcher",
                StringComparison.OrdinalIgnoreCase) ||
            routePath.StartsWith("/dispatch/", StringComparison.OrdinalIgnoreCase))
            return "Dispatcher";

        if (routePath.Equals(
                "/dashboard/courier",
                StringComparison.OrdinalIgnoreCase) ||
            routePath.StartsWith("/courier/", StringComparison.OrdinalIgnoreCase))
            return "Courier";

        return null;
    }
}
