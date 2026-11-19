using System.Linq.Expressions;
using CourierParcelTrackingSystem.API.Entities;

namespace CourierParcelTrackingSystem.API.Repositories;

internal static class ParcelAssignmentEligibility
{
    public static Expression<Func<Parcel, bool>> InitialAssignment =>
        parcel =>
            parcel.IsActive &&
            !parcel.IsDeleted &&
            parcel.CurrentStatus.IsActive &&
            !parcel.CurrentStatus.IsDeleted &&
            parcel.CurrentStatus.StatusCode == "BOOKED" &&
            !parcel.DeliveredAt.HasValue &&
            !parcel.ParcelAssignments.Any(assignment =>
                assignment.UnassignedAt == null);
}
