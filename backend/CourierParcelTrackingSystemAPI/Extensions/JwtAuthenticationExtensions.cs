using System.Security.Claims;
using System.Text;
using CourierParcelTrackingSystem.API.Data;
using CourierParcelTrackingSystem.API.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CourierParcelTrackingSystem.API.Extensions;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings configuration section is missing.");

        var secret = configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < 32)
            throw new InvalidOperationException(
                "Configure JwtSettings:SecretKey as a private random secret of at least 32 bytes using User Secrets or environment variables.");

        if (string.IsNullOrWhiteSpace(settings.Issuer) ||
            string.IsNullOrWhiteSpace(settings.Audience) || settings.ExpiryMinutes <= 0)
            throw new InvalidOperationException(
                "JwtSettings must contain Issuer, Audience, and a positive ExpiryMinutes value.");

        settings.SecretKey = secret;
        services.Configure<JwtSettings>(options =>
        {
            options.SecretKey = secret;
            options.Issuer = settings.Issuer;
            options.Audience = settings.Audience;
            options.ExpiryMinutes = settings.ExpiryMinutes;
        });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,
                ValidateAudience = true,
                ValidAudience = settings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Name
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var userIdClaim = context.Principal?
                        .FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!int.TryParse(userIdClaim, out var userId))
                    {
                        context.Fail("The access token has no valid user identifier.");
                        return;
                    }

                    var dbContext = context.HttpContext.RequestServices
                        .GetRequiredService<ApplicationDbContext>();
                    var user = await dbContext.Users
                        .AsNoTracking()
                        .Where(user =>
                        user.UserId == userId &&
                        user.IsActive &&
                        !user.IsDeleted)
                        .Select(user => new
                        {
                            RoleNames = user.UserRoleUsers
                                .Where(userRole =>
                                    userRole.Role.IsActive &&
                                    !userRole.Role.IsDeleted)
                                .Select(userRole => userRole.Role.RoleName)
                                .Distinct()
                                .ToList()
                        })
                        .FirstOrDefaultAsync();
                    if (user is null)
                    {
                        context.Fail("The user account is inactive.");
                        return;
                    }

                    var tokenRoles = context.Principal!
                        .FindAll(ClaimTypes.Role)
                        .Select(claim => claim.Value)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var activeRoles = user.RoleNames.ToHashSet(
                        StringComparer.OrdinalIgnoreCase);
                    if (!tokenRoles.SetEquals(activeRoles))
                        context.Fail("The user's role assignments changed. Sign in again.");
                }
            };
        });

        services.AddAuthorization();
        return services;
    }
}
