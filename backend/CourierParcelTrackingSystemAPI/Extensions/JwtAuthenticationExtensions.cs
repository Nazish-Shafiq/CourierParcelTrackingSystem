using System.Security.Claims;
using System.Text;
using CourierParcelTrackingSystem.API.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        });

        services.AddAuthorization();
        return services;
    }
}
