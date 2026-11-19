
using Microsoft.EntityFrameworkCore;
using CourierParcelTrackingSystem.API.Data;
using CourierParcelTrackingSystem.API.DTOs.ReportDTO;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;

namespace CourierParcelTrackingSystem.API.Repositories.Implementations;

public class ReportsRepository : IReportsRepository
{
    private readonly ApplicationDbContext _context;

    public ReportsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Report 1: Parcel booking details.
    public async Task<List<ParcelBookingReportDto>>
        GetParcelBookingReportAsync(ReportRequestDto request)
    {
        var query = _context.Parcels
            .AsNoTracking()
            .Where(p => p.IsActive && !p.IsDeleted);

        if (request.FromDate.HasValue)
        {
            var fromDate = request.FromDate.Value.Date;

            query = query.Where(p => p.BookingDate >= fromDate);
        }

        if (request.ToDate.HasValue)
        {
            var toDateExclusive = request.ToDate.Value.Date.AddDays(1);

            query = query.Where(p => p.BookingDate < toDateExclusive);
        }

        return await query
            .OrderByDescending(p => p.BookingDate)
            .Select(p => new ParcelBookingReportDto
            {
                ParcelId = p.ParcelId,
                TrackingNumber = p.TrackingNumber,
                SenderName = p.SenderCustomer.FullName,

                RecipientName = p.ParcelRecipient != null
                    ? p.ParcelRecipient.RecipientName
                    : string.Empty,

                RecipientCity = p.ParcelRecipient != null
                    ? p.ParcelRecipient.City
                    : string.Empty,

                CurrentStatus = p.CurrentStatus.StatusName,
                BookingDate = p.BookingDate,
                ExpectedDeliveryDate = p.ExpectedDeliveryDate,
                TotalWeightKg = p.TotalWeightKg
            })
            .ToListAsync();
    }

    // Report 2: Parcel counts grouped by current status.
    public async Task<List<ParcelStatusReportDto>>
        GetParcelStatusReportAsync(ReportRequestDto request)
    {
        var parcels = _context.Parcels
            .AsNoTracking()
            .Where(p => p.IsActive && !p.IsDeleted);

        if (request.FromDate.HasValue)
        {
            var fromDate = request.FromDate.Value.Date;

            parcels = parcels.Where(p => p.BookingDate >= fromDate);
        }

        if (request.ToDate.HasValue)
        {
            var toDateExclusive = request.ToDate.Value.Date.AddDays(1);

            parcels = parcels.Where(p => p.BookingDate < toDateExclusive);
        }

        return await _context.ParcelStatuses
            .AsNoTracking()
            .Where(s => s.IsActive && !s.IsDeleted)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new ParcelStatusReportDto
            {
                ParcelStatusId = s.ParcelStatusId,
                StatusCode = s.StatusCode,
                StatusName = s.StatusName,

                TotalParcels = parcels.Count(p =>
                    p.CurrentStatusId == s.ParcelStatusId)
            })
            .ToListAsync();
    }

    // Report 3: Courier assignment and delivery performance.
    public async Task<List<CourierPerformanceReportDto>>
        GetCourierPerformanceReportAsync(ReportRequestDto request)
    {
        var couriers = _context.Couriers
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        return await couriers
            .OrderBy(c => c.FullName)
            .Select(c => new CourierPerformanceReportDto
            {
                CourierId = c.CourierId,
                EmployeeCode = c.EmployeeCode,
                CourierName = c.FullName,
                Email = c.Email,
                Phone = c.Phone,
                HubName = c.Hub != null ? c.Hub.HubName : null,
                VehicleType = c.VehicleType,
                VehicleNumber = c.VehicleNumber,
                IsAvailable = c.IsAvailable,
                IsActive = c.IsActive,

                TotalAssignedParcels = c.ParcelAssignments
                    .Where(a =>
                        a.Parcel.IsActive &&
                        !a.Parcel.IsDeleted &&
                        (!request.FromDate.HasValue ||
                            a.AssignedAt >= request.FromDate.Value.Date) &&
                        (!request.ToDate.HasValue ||
                            a.AssignedAt <
                            request.ToDate.Value.Date.AddDays(1)))
                    .Select(a => a.ParcelId)
                    .Distinct()
                    .Count(),

                ActiveAssignments = c.ParcelAssignments
                    .Count(a =>
                        a.UnassignedAt == null &&
                        a.Parcel.IsActive &&
                        !a.Parcel.IsDeleted),

                DeliveredParcels = c.StatusEvents
                    .Where(e =>
                        e.Parcel.IsActive &&
                        !e.Parcel.IsDeleted &&
                        e.ParcelStatus.StatusCode == "DELIVERED" &&
                        (!request.FromDate.HasValue ||
                            e.EventAt >= request.FromDate.Value.Date) &&
                        (!request.ToDate.HasValue ||
                            e.EventAt <
                            request.ToDate.Value.Date.AddDays(1)))
                    .Select(e => e.ParcelId)
                    .Distinct()
                    .Count()
            })
            .ToListAsync();
    }

    public async Task<DeliveryReportDto> GetDeliveryReportAsync(
        ReportRequestDto request)
    {
        var query = _context.StatusEvents
            .AsNoTracking()
            .Where(e =>
                e.Parcel.IsActive &&
                !e.Parcel.IsDeleted &&
                !e.ParcelStatus.IsDeleted &&
                (e.ParcelStatus.StatusCode == "DELIVERED" ||
                    e.ParcelStatus.StatusCode == "DELIVERY_ATTEMPTED"));

        if (request.FromDate.HasValue)
        {
            var fromDate = request.FromDate.Value.Date;
            query = query.Where(e => e.EventAt >= fromDate);
        }

        if (request.ToDate.HasValue)
        {
            var toDateExclusive = request.ToDate.Value.Date.AddDays(1);
            query = query.Where(e => e.EventAt < toDateExclusive);
        }

        var events = await query
            .OrderByDescending(e => e.EventAt)
            .Select(e => new DeliveryReportEventDto
            {
                StatusEventId = e.StatusEventId,
                ParcelId = e.ParcelId,
                TrackingNumber = e.Parcel.TrackingNumber,
                StatusCode = e.ParcelStatus.StatusCode,
                StatusName = e.ParcelStatus.StatusName,
                CourierName = e.Courier != null ? e.Courier.FullName : null,
                HubName = e.Hub != null ? e.Hub.HubName : null,
                EventAt = e.EventAt
            })
            .ToListAsync();

        return new DeliveryReportDto
        {
            SuccessfulDeliveries = events.Count(e => e.StatusCode == "DELIVERED"),
            FailedDeliveryAttempts = events.Count(e =>
                e.StatusCode == "DELIVERY_ATTEMPTED"),
            Events = events
        };
    }

    public async Task<List<HubParcelReportDto>> GetHubParcelReportAsync(
        ReportRequestDto request)
    {
        var parcels = _context.Parcels
            .AsNoTracking()
            .Where(p => p.IsActive && !p.IsDeleted);

        if (request.FromDate.HasValue)
        {
            var fromDate = request.FromDate.Value.Date;
            parcels = parcels.Where(p => p.BookingDate >= fromDate);
        }

        if (request.ToDate.HasValue)
        {
            var toDateExclusive = request.ToDate.Value.Date.AddDays(1);
            parcels = parcels.Where(p => p.BookingDate < toDateExclusive);
        }

        return await _context.Hubs
            .AsNoTracking()
            .Where(h => !h.IsDeleted)
            .OrderBy(h => h.HubName)
            .Select(h => new HubParcelReportDto
            {
                HubId = h.HubId,
                HubName = h.HubName,
                HubCode = h.HubCode,
                City = h.City,
                Province = h.Province,
                IsActive = h.IsActive,
                OriginParcelCount = parcels.Count(p =>
                    p.Route != null && p.Route.OriginHubId == h.HubId),
                DestinationParcelCount = parcels.Count(p =>
                    p.Route != null && p.Route.DestinationHubId == h.HubId)
            })
            .ToListAsync();
    }

    public async Task<UserReportDto> GetUserReportAsync()
    {
        var users = _context.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted);

        var totalUsers = await users.CountAsync();
        var activeUsers = await users.CountAsync(u => u.IsActive);
        var inactiveUsers = totalUsers - activeUsers;

        var roles = await _context.Roles
            .AsNoTracking()
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.RoleName)
            .Select(r => new UserRoleReportDto
            {
                RoleName = r.RoleName,
                TotalUsers = r.UserRoles.Count(ur =>
                    !ur.User.IsDeleted),
                ActiveUsers = r.UserRoles.Count(ur =>
                    !ur.User.IsDeleted && ur.User.IsActive),
                InactiveUsers = r.UserRoles.Count(ur =>
                    !ur.User.IsDeleted && !ur.User.IsActive)
            })
            .ToListAsync();

        return new UserReportDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            InactiveUsers = inactiveUsers,
            Roles = roles
        };
    }
}
