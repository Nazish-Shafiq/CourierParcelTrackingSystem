using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Services;

public static class ParcelAssignmentHubResolver
{
    public static Hub? ResolveOperationalHub(Parcel parcel)
    {
        if (parcel.CurrentHubId.HasValue)
            return parcel.CurrentHub;

        var route = parcel.Route;
        if (route is null || !route.IsActive || route.IsDeleted)
            return null;

        var statusCode = parcel.CurrentStatus?.StatusCode;
        var isFinalDelivery = statusCode is not null &&
            (statusCode.Equals("AT_DESTINATION_HUB", StringComparison.OrdinalIgnoreCase) ||
             statusCode.Equals("OUT_FOR_DELIVERY", StringComparison.OrdinalIgnoreCase) ||
             statusCode.Equals("DELIVERY_ATTEMPTED", StringComparison.OrdinalIgnoreCase));

        return isFinalDelivery
            ? route.DestinationHub
            : route.OriginHub;
    }

    public static void EnsureParcelCanBeAssigned(Parcel parcel)
    {
        if (parcel.DeliveredAt.HasValue ||
            parcel.CurrentStatus.IsTerminal ||
            parcel.CurrentStatus.StatusCode.Equals(
                "DELIVERED",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "A delivered or terminal parcel cannot be assigned or reassigned.");
        }
    }
}
