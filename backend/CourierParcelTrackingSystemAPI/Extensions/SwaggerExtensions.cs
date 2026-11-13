
using Microsoft.OpenApi.Models;

namespace CourierParcelTrackingSystem.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Courier Parcel Tracking System API",
                Version = "v1"
            });

            // Generate unique and readable schema IDs.
            options.CustomSchemaIds(type =>
            {
                if (!type.IsGenericType)
                {
                    return $"{type.Namespace}_{type.Name}";
                }

                var genericTypeName = type.Name.Split('`')[0];

                var genericArguments = string.Join(
                    "_",
                    type.GetGenericArguments().Select(GetUniqueSchemaName)
                );

                return $"{type.Namespace}_{genericTypeName}_{genericArguments}";
            });

            // JWT Bearer Authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste the access token returned by /api/auth/login."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    // Helper method for generating unique names for generic types.
    private static string GetUniqueSchemaName(Type type)
    {
        if (!type.IsGenericType)
        {
            return $"{type.Namespace}_{type.Name}";
        }

        var genericTypeName = type.Name.Split('`')[0];

        var genericArguments = string.Join(
            "_",
            type.GetGenericArguments().Select(GetUniqueSchemaName)
        );

        return $"{type.Namespace}_{genericTypeName}_{genericArguments}";
    }
}
