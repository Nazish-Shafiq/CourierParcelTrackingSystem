namespace CourierParcelTrackingSystem.API.Services;

public static class ParcelStatusTransitionRules
{
    public static bool IsCourierAllowed(string currentCode, string targetCode)
    {
        var current = Normalize(currentCode);
        var target = Normalize(targetCode);

        return current switch
        {
            "BOOKED" => target == "PICKED_UP",
            "PICKED_UP" => target == "IN_TRANSIT",
            "IN_TRANSIT" => target == "AT_DESTINATION_HUB",
            "AT_DESTINATION_HUB" => target == "OUT_FOR_DELIVERY",
            "DELIVERY_ATTEMPTED" => target == "OUT_FOR_DELIVERY",
            "OUT_FOR_DELIVERY" => target is "DELIVERED" or "DELIVERY_ATTEMPTED",
            _ => false
        };
    }

    public static bool IsGeneralAllowed(
        string currentCode,
        string targetCode,
        string? statusBeforeHold = null)
    {
        var current = Normalize(currentCode);
        var target = Normalize(targetCode);

        if (current == target)
            return false;

        if (current == "ON_HOLD")
        {
            var resumeCode = Normalize(statusBeforeHold);
            return target is "RETURNED" or "CANCELLED" ||
                   (!string.IsNullOrEmpty(resumeCode) && target == resumeCode);
        }

        return current switch
        {
            "BOOKED" => target is "PICKED_UP" or "ON_HOLD" or "CANCELLED",
            "PICKED_UP" => target is "AT_ORIGIN_HUB" or "IN_TRANSIT" or "ON_HOLD" or "CANCELLED",
            "AT_ORIGIN_HUB" => target is "IN_TRANSIT" or "ON_HOLD" or "RETURNED" or "CANCELLED",
            "IN_TRANSIT" => target is "AT_DESTINATION_HUB" or "ON_HOLD" or "RETURNED" or "CANCELLED",
            "AT_DESTINATION_HUB" => target is "OUT_FOR_DELIVERY" or "ON_HOLD" or "RETURNED" or "CANCELLED",
            "OUT_FOR_DELIVERY" => target is "DELIVERED" or "DELIVERY_ATTEMPTED" or "ON_HOLD" or "RETURNED" or "CANCELLED",
            "DELIVERY_ATTEMPTED" => target is "OUT_FOR_DELIVERY" or "ON_HOLD" or "RETURNED" or "CANCELLED",
            "DELIVERED" or "RETURNED" or "CANCELLED" => false,
            _ => false
        };
    }

    public static bool IsTerminal(string code) =>
        Normalize(code) is "DELIVERED" or "RETURNED" or "CANCELLED";

    private static string Normalize(string? code) =>
        code?.Trim().ToUpperInvariant() ?? string.Empty;
}
