
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
            .Where(c => c.IsActive && !c.IsDeleted);

        return await couriers
            .OrderBy(c => c.FullName)
            .Select(c => new CourierPerformanceReportDto
            {
                CourierId = c.CourierId,
                EmployeeCode = c.EmployeeCode,
                CourierName = c.FullName,

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

                DeliveredParcels = c.ParcelAssignments
                    .Where(a =>
                        a.Parcel.IsActive &&
                        !a.Parcel.IsDeleted &&
                        a.Parcel.CurrentStatus.StatusCode == "DELIVERED" &&
                        (!request.FromDate.HasValue ||
                            a.AssignedAt >= request.FromDate.Value.Date) &&
                        (!request.ToDate.HasValue ||
                            a.AssignedAt <
                            request.ToDate.Value.Date.AddDays(1)))
                    .Select(a => a.ParcelId)
                    .Distinct()
                    .Count()
            })
            .ToListAsync();
    }
}
