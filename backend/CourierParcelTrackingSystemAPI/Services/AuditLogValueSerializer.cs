using System.Text.Json;

namespace CourierParcelTrackingSystem.API.Services;

internal static class AuditLogValueSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = null
    };

    public static bool IsSensitiveProperty(string propertyName)
    {
        var name = propertyName.Replace("_", string.Empty);
        return name.Contains("password", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("token", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("hash", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("credential", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("securitystamp", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("connectionstring", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("privatekey", StringComparison.OrdinalIgnoreCase);
    }

    public static string? Serialize(
        IReadOnlyDictionary<string, object?>? values)
    {
        if (values is null || values.Count == 0)
            return null;

        var safeValues = values
            .Where(item => !IsSensitiveProperty(item.Key))
            .ToDictionary(item => item.Key, item => item.Value);

        return safeValues.Count == 0
            ? null
            : JsonSerializer.Serialize(safeValues, SerializerOptions);
    }
}
