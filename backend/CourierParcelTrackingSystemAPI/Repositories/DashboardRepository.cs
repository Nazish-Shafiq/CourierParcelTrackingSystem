
using Microsoft.EntityFrameworkCore;

using CourierParcelTrackingSystem.API.Data;
using CourierParcelTrackingSystem.API.DTOs.DashboardDTO;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;

namespace CourierParcelTrackingSystem.API.Repositories.Implementations;

public class DashboardRepository : IDashboardRepository
{
    private readonly ApplicationDbContext _context;

    public DashboardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardResponseDto> GetAdminDashboardAsync()
    {
        // Count active and non-deleted parcels.
        var totalParcels = await _context.Parcels
            .CountAsync(p => p.IsActive && !p.IsDeleted);

        // Count active and non-deleted customers.
        var totalCustomers = await _context.Customers
            .CountAsync(c => c.IsActive && !c.IsDeleted);

        // Count active and non-deleted couriers.
        var totalCouriers = await _context.Couriers
            .CountAsync(c => c.IsActive && !c.IsDeleted);

        // Count available couriers.
        var availableCouriers = await _context.Couriers
            .CountAsync(c => c.IsActive && !c.IsDeleted && c.IsAvailable);

        // Count busy couriers.
        var busyCouriers = await _context.Couriers
            .CountAsync(c => c.IsActive && !c.IsDeleted && !c.IsAvailable);

        // Count active hubs.
        var totalHubs = await _context.Hubs
            .CountAsync(h => h.IsActive && !h.IsDeleted);

        // Count active routes.
        var totalRoutes = await _context.Routes
            .CountAsync(r => r.IsActive && !r.IsDeleted);

        // Get all active statuses with their parcel counts.
        var statusSummary = await _context.ParcelStatuses
            .AsNoTracking()
            .Where(s => s.IsActive && !s.IsDeleted)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new ParcelStatusSummaryDto
            {
                ParcelStatusId = s.ParcelStatusId,
                StatusName = s.StatusName,
                StatusCode = s.StatusCode,

                TotalParcels = s.Parcels.Count(p =>
                    p.IsActive && !p.IsDeleted)
            })
            .ToListAsync();

        // Get the five most recent parcel bookings.
        var recentBookings = await _context.Parcels
            .AsNoTracking()
            .Where(p => p.IsActive && !p.IsDeleted)
            .OrderByDescending(p => p.BookingDate)
            .Take(5)
            .Select(p => new RecentParcelBookingDto
            {
                ParcelId = p.ParcelId,
                TrackingNumber = p.TrackingNumber,
                SenderName = p.SenderCustomer.FullName,
                StatusName = p.CurrentStatus.StatusName,
                BookingDate = p.BookingDate,
                ExpectedDeliveryDate = p.ExpectedDeliveryDate
            })
            .ToListAsync();

        return new AdminDashboardResponseDto
        {
            TotalParcels = totalParcels,
            TotalCustomers = totalCustomers,
            TotalCouriers = totalCouriers,
            AvailableCouriers = availableCouriers,
            BusyCouriers = busyCouriers,
            TotalHubs = totalHubs,
            TotalRoutes = totalRoutes,
            ParcelStatusSummary = statusSummary,
            RecentBookings = recentBookings
        };
    }


    public async Task<DispatcherDashboardResponseDto> GetDispatcherDashboardAsync()
    {
        // Count all active parcels.
        var totalParcels = await _context.Parcels
            .CountAsync(p => p.IsActive && !p.IsDeleted);

        // Count parcels without an active assignment.
        // Exclude terminal-status parcels from assignment requirements.
        var unassignedParcels = await _context.Parcels
            .CountAsync(p =>
                p.IsActive &&
                !p.IsDeleted &&
                !p.CurrentStatus.IsTerminal &&
                !p.ParcelAssignments.Any(a => a.UnassignedAt == null));

        // Count currently active assignments.
        var activeAssignments = await _context.ParcelAssignments
            .CountAsync(a =>
                a.UnassignedAt == null &&
                a.Parcel.IsActive &&
                !a.Parcel.IsDeleted);

        // Count parcels currently in transit.
        var parcelsInTransit = await _context.Parcels
            .CountAsync(p =>
                p.IsActive &&
                !p.IsDeleted &&
                p.CurrentStatus.StatusCode == "IN_TRANSIT");

        // Count available couriers.
        var availableCouriers = await _context.Couriers
            .CountAsync(c =>
                c.IsActive &&
                !c.IsDeleted &&
                c.IsAvailable);

        // Count busy couriers.
        var busyCouriers = await _context.Couriers
            .CountAsync(c =>
                c.IsActive &&
                !c.IsDeleted &&
                !c.IsAvailable);

        // Get the five most recent active assignments.
        var recentAssignments = await _context.ParcelAssignments
            .AsNoTracking()
            .Where(a =>
                a.UnassignedAt == null &&
                a.Parcel.IsActive &&
                !a.Parcel.IsDeleted)
            .OrderByDescending(a => a.AssignedAt)
            .Take(5)
            .Select(a => new RecentAssignmentDto
            {
                ParcelAssignmentId = a.ParcelAssignmentId,
                ParcelId = a.ParcelId,
                TrackingNumber = a.Parcel.TrackingNumber,
                CourierId = a.CourierId,
                CourierName = a.Courier.FullName,
                AssignedAt = a.AssignedAt,
                AssignmentNotes = a.AssignmentNotes
            })
            .ToListAsync();

        // Get the five most recent parcels waiting for assignment.
        var recentUnassignedParcels = await _context.Parcels
            .AsNoTracking()
            .Where(p =>
                p.IsActive &&
                !p.IsDeleted &&
                !p.CurrentStatus.IsTerminal &&
                !p.ParcelAssignments.Any(a => a.UnassignedAt == null))
            .OrderByDescending(p => p.BookingDate)
            .Take(5)
            .Select(p => new UnassignedParcelDto
            {
                ParcelId = p.ParcelId,
                TrackingNumber = p.TrackingNumber,
                SenderName = p.SenderCustomer.FullName,
                StatusName = p.CurrentStatus.StatusName,
                BookingDate = p.BookingDate
            })
            .ToListAsync();

        return new DispatcherDashboardResponseDto
        {
            TotalParcels = totalParcels,
            UnassignedParcels = unassignedParcels,
            ActiveAssignments = activeAssignments,
            ParcelsInTransit = parcelsInTransit,
            AvailableCouriers = availableCouriers,
            BusyCouriers = busyCouriers,
            RecentAssignments = recentAssignments,
            RecentUnassignedParcels = recentUnassignedParcels
        };
    }

    public async Task<CourierDashboardResponseDto?> GetCourierDashboardAsync(int userId)
    {
        var dashboard = await _context.Couriers
            .AsNoTracking()
            .Where(c =>
                c.UserId == userId &&
                c.IsActive &&
                !c.IsDeleted)
            .Select(c => new CourierDashboardResponseDto
            {
                CourierName = c.FullName,
                EmployeeCode = c.EmployeeCode,

                TotalAssignedParcels = c.ParcelAssignments.Count(a =>
                    a.UnassignedAt == null &&
                    a.Parcel.IsActive &&
                    !a.Parcel.IsDeleted),

                ParcelsInTransit = c.ParcelAssignments.Count(a =>
                    a.UnassignedAt == null &&
                    a.Parcel.IsActive &&
                    !a.Parcel.IsDeleted &&
                    a.Parcel.CurrentStatus.StatusCode == "IN_TRANSIT"),

                ParcelsDelivered = c.ParcelAssignments.Count(a =>
                    a.UnassignedAt == null &&
                    a.Parcel.IsActive &&
                    !a.Parcel.IsDeleted &&
                    a.Parcel.CurrentStatus.StatusCode == "DELIVERED"),

                AssignedParcels = c.ParcelAssignments
                    .Where(a =>
                        a.UnassignedAt == null &&
                        a.Parcel.IsActive &&
                        !a.Parcel.IsDeleted)
                    .OrderByDescending(a => a.AssignedAt)
                    .Select(a => new CourierAssignedParcelDto
                    {
                        ParcelAssignmentId = a.ParcelAssignmentId,
                        ParcelId = a.ParcelId,
                        TrackingNumber = a.Parcel.TrackingNumber,

                        SenderName = a.Parcel.SenderCustomer.FullName,

                        RecipientName = a.Parcel.ParcelRecipient != null
                            ? a.Parcel.ParcelRecipient.RecipientName
                            : string.Empty,

                        RecipientCity = a.Parcel.ParcelRecipient != null
                            ? a.Parcel.ParcelRecipient.City
                            : string.Empty,

                        CurrentStatus = a.Parcel.CurrentStatus.StatusName,
                        AssignedAt = a.AssignedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        return dashboard;
    }

}
