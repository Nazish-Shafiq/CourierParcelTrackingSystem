
﻿using System.Data;
using Microsoft.EntityFrameworkCore;

using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Data; // Adjust if your ApplicationDbContext uses another namespace.

namespace CourierParcelTrackingSystem.API.Repositories.Implementations;

public class ParcelTrackingRepository : IParcelTrackingRepository
{
    private readonly ApplicationDbContext _context;

    public ParcelTrackingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Get all tracking events with pagination.
    public async Task<PagedResponseDto<StatusEvent>> GetPagedAsync(
        PaginationRequestDto request,
        int? courierUserId = null)
    {
        IQueryable<StatusEvent> query = IncludeTrackingDetails(
            _context.StatusEvents.AsNoTracking())
            .Where(e => e.Parcel.IsActive && !e.Parcel.IsDeleted);

        if (courierUserId.HasValue)
        {
            var accessibleParcelIds = GetCourierAccessibleParcels(
                courierUserId.Value)
                .Select(p => p.ParcelId);
            query = query.Where(e => accessibleParcelIds.Contains(e.ParcelId));
        }

        var totalRecords = await query.CountAsync();

        var items = await query
            .OrderByDescending(e => e.EventAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResponseDto<StatusEvent>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(
                (double)totalRecords / request.PageSize)
        };
    }

    // Get complete tracking history using tracking number.
    public async Task<List<StatusEvent>> GetTrackingHistoryAsync(
        string trackingNumber,
        int? courierUserId = null)
    {
        var query = IncludeTrackingDetails(_context.StatusEvents.AsNoTracking())
            .Where(e =>
                e.Parcel.TrackingNumber == trackingNumber &&
                e.Parcel.IsActive &&
                !e.Parcel.IsDeleted);

        if (courierUserId.HasValue)
        {
            var accessibleParcelIds = GetCourierAccessibleParcels(
                courierUserId.Value)
                .Select(p => p.ParcelId);
            query = query.Where(e => accessibleParcelIds.Contains(e.ParcelId));
        }

        return await query
            .OrderBy(e => e.EventAt)
            .ThenBy(e => e.StatusEventId)
            .ToListAsync();
    }

    public async Task<bool> HasAccessibleParcelForTrackingAsync(
        string trackingNumber,
        int? courierUserId = null)
    {
        IQueryable<Parcel> query = _context.Parcels
            .Where(p =>
                p.TrackingNumber == trackingNumber &&
                p.IsActive &&
                !p.IsDeleted);

        if (courierUserId.HasValue)
        {
            var accessibleParcelIds = GetCourierAccessibleParcels(
                courierUserId.Value)
                .Select(p => p.ParcelId);
            query = query.Where(p => accessibleParcelIds.Contains(p.ParcelId));
        }

        return await query.AnyAsync();
    }

    private IQueryable<Parcel> GetCourierAccessibleParcels(int courierUserId) =>
        _context.Parcels.Where(p =>
            p.ParcelAssignments.Any(a =>
                a.UnassignedAt == null &&
                a.Courier.UserId == courierUserId &&
                a.Courier.IsActive &&
                !a.Courier.IsDeleted &&
                a.Courier.User != null &&
                a.Courier.User.IsActive &&
                !a.Courier.User.IsDeleted) ||
            p.StatusEvents.Any(e =>
                e.ParcelStatus.StatusCode == "DELIVERED" &&
                e.CourierId.HasValue &&
                e.Courier != null &&
                e.Courier.UserId == courierUserId &&
                e.Courier.IsActive &&
                !e.Courier.IsDeleted &&
                e.Courier.User != null &&
                e.Courier.User.IsActive &&
                !e.Courier.User.IsDeleted));

    // Get a parcel as a tracked entity for status updates.
    public async Task<Parcel?> GetParcelForUpdateAsync(long parcelId)
    {
        return await _context.Parcels
            .Include(p => p.CurrentStatus)
            .FirstOrDefaultAsync(p =>
                p.ParcelId == parcelId &&
                p.IsActive &&
                !p.IsDeleted);
    }

    public async Task<Courier?> GetActiveCourierForUserAsync(int userId)
    {
        return await _context.Couriers
            .FirstOrDefaultAsync(c =>
                c.UserId == userId &&
                c.IsActive &&
                !c.IsDeleted &&
                c.User != null &&
                c.User.IsActive &&
                !c.User.IsDeleted);
    }

    public async Task<ParcelAssignment?> GetActiveAssignmentForCourierAsync(
        long parcelId,
        int courierId)
    {
        return await _context.ParcelAssignments
            .Include(a => a.Parcel)
            .FirstOrDefaultAsync(a =>
                a.ParcelId == parcelId &&
                a.CourierId == courierId &&
                a.UnassignedAt == null &&
                a.Parcel.IsActive &&
                !a.Parcel.IsDeleted &&
                a.Courier.IsActive &&
                !a.Courier.IsDeleted &&
                a.Courier.User != null &&
                a.Courier.User.IsActive &&
                !a.Courier.User.IsDeleted);
    }

    // Get an active parcel status.
    public async Task<ParcelStatus?> GetActiveStatusAsync(
        int parcelStatusId)
    {
        return await _context.ParcelStatuses
            .FirstOrDefaultAsync(s =>
                s.ParcelStatusId == parcelStatusId &&
                s.IsActive &&
                !s.IsDeleted);
    }

    public async Task<ParcelStatus?> GetActiveStatusByCodeAsync(string statusCode)
    {
        return await _context.ParcelStatuses
            .FirstOrDefaultAsync(s =>
                s.StatusCode == statusCode &&
                s.IsActive &&
                !s.IsDeleted);
    }

    public async Task<DateTime?> GetLatestStatusEventAtAsync(
        long parcelId,
        string statusCode)
    {
        return await _context.StatusEvents
            .Where(e =>
                e.ParcelId == parcelId &&
                e.ParcelStatus.StatusCode == statusCode)
            .OrderByDescending(e => e.EventAt)
            .ThenByDescending(e => e.StatusEventId)
            .Select(e => (DateTime?)e.EventAt)
            .FirstOrDefaultAsync();
    }

    public async Task<string?> GetStatusCodeBeforeOnHoldAsync(long parcelId)
    {
        var latestOnHoldAt = await _context.StatusEvents
            .Where(e =>
                e.ParcelId == parcelId &&
                e.ParcelStatus.StatusCode == "ON_HOLD")
            .OrderByDescending(e => e.EventAt)
            .ThenByDescending(e => e.StatusEventId)
            .Select(e => (DateTime?)e.EventAt)
            .FirstOrDefaultAsync();

        if (!latestOnHoldAt.HasValue)
            return null;

        return await _context.StatusEvents
            .Where(e =>
                e.ParcelId == parcelId &&
                e.EventAt < latestOnHoldAt.Value &&
                e.ParcelStatus.StatusCode != "ON_HOLD")
            .OrderByDescending(e => e.EventAt)
            .ThenByDescending(e => e.StatusEventId)
            .Select(e => e.ParcelStatus.StatusCode)
            .FirstOrDefaultAsync();
    }

    public async Task<StatusEvent?> TryApplyCourierStatusTransitionAsync(
        long parcelId,
        int expectedStatusId,
        int targetStatusId,
        string targetStatusCode,
        long parcelAssignmentId,
        int courierId,
        int userId,
        string? notes,
        DateTime eventAt,
        bool requiresDispatcherReassignment)
    {
        long statusEventId;
        var isDelivered = string.Equals(
            targetStatusCode,
            "DELIVERED",
            StringComparison.OrdinalIgnoreCase);
        var isDeliveryAttempted = string.Equals(
            targetStatusCode,
            "DELIVERY_ATTEMPTED",
            StringComparison.OrdinalIgnoreCase);

        await using (var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable))
        {
            try
            {
                var targetIsActive = await _context.ParcelStatuses
                    .AnyAsync(s =>
                        s.ParcelStatusId == targetStatusId &&
                        s.StatusCode == targetStatusCode &&
                        s.IsActive &&
                        !s.IsDeleted);

                if (!targetIsActive)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                if (requiresDispatcherReassignment)
                {
                    var latestAttemptAt = await _context.StatusEvents
                        .Where(e =>
                            e.ParcelId == parcelId &&
                            e.ParcelStatus.StatusCode == "DELIVERY_ATTEMPTED")
                        .OrderByDescending(e => e.EventAt)
                        .ThenByDescending(e => e.StatusEventId)
                        .Select(e => (DateTime?)e.EventAt)
                        .FirstOrDefaultAsync();

                    var assignmentAt = await _context.ParcelAssignments
                        .Where(a =>
                            a.ParcelAssignmentId == parcelAssignmentId &&
                            a.CourierId == courierId &&
                            a.UnassignedAt == null)
                        .Select(a => (DateTime?)a.AssignedAt)
                        .FirstOrDefaultAsync();

                    if (!latestAttemptAt.HasValue ||
                        !assignmentAt.HasValue ||
                        assignmentAt.Value <= latestAttemptAt.Value)
                    {
                        await transaction.RollbackAsync();
                        return null;
                    }
                }

                var updatedParcels = await _context.Parcels
                    .Where(p =>
                        p.ParcelId == parcelId &&
                        p.CurrentStatusId == expectedStatusId &&
                        p.CurrentStatus.IsActive &&
                        !p.CurrentStatus.IsDeleted &&
                        p.IsActive &&
                        !p.IsDeleted &&
                        p.ParcelAssignments.Any(a =>
                            a.ParcelAssignmentId == parcelAssignmentId &&
                            a.CourierId == courierId &&
                            a.UnassignedAt == null &&
                            a.Courier.IsActive &&
                            !a.Courier.IsDeleted &&
                            a.Courier.UserId == userId &&
                            a.Courier.User != null &&
                            a.Courier.User.IsActive &&
                            !a.Courier.User.IsDeleted))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.CurrentStatusId, targetStatusId)
                        .SetProperty(p => p.UpdatedAt, eventAt)
                        .SetProperty(p => p.UpdatedBy, (int?)userId)
                        .SetProperty(
                            p => p.DeliveredAt,
                            p => isDelivered ? eventAt : p.DeliveredAt));

                if (updatedParcels != 1)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                if (isDelivered || isDeliveryAttempted)
                {
                    var endedAssignments = await _context.ParcelAssignments
                        .Where(a =>
                            a.ParcelAssignmentId == parcelAssignmentId &&
                            a.CourierId == courierId &&
                            a.UnassignedAt == null)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(a => a.UnassignedAt, eventAt)
                            .SetProperty(a => a.UnassignedBy, (int?)userId));

                    var hasOtherActiveAssignments = await _context.ParcelAssignments
                        .AnyAsync(a =>
                            a.CourierId == courierId &&
                            a.UnassignedAt == null);
                    var releasedCouriers = await _context.Couriers
                        .Where(c =>
                            c.CourierId == courierId &&
                            c.UserId == userId &&
                            c.IsActive &&
                            !c.IsDeleted)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(
                                c => c.IsAvailable,
                                !hasOtherActiveAssignments)
                            .SetProperty(c => c.UpdatedAt, eventAt)
                            .SetProperty(c => c.UpdatedBy, (int?)userId));

                    if (endedAssignments != 1 || releasedCouriers != 1)
                    {
                        throw new DbUpdateConcurrencyException(
                            "The Courier assignment changed before the status could be recorded.");
                    }
                }

                var statusEvent = new StatusEvent
                {
                    ParcelId = parcelId,
                    ParcelStatusId = targetStatusId,
                    HubId = await _context.Parcels
                        .Where(p => p.ParcelId == parcelId)
                        .Select(p => p.CurrentHubId)
                        .SingleOrDefaultAsync(),
                    CourierId = courierId,
                    EventAt = eventAt,
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                    CreatedAt = eventAt,
                    CreatedBy = userId
                };

                await _context.StatusEvents.AddAsync(statusEvent);
                await _context.SaveChangesAsync();
                statusEventId = statusEvent.StatusEventId;
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        return await IncludeTrackingDetails(_context.StatusEvents.AsNoTracking())
            .SingleAsync(e => e.StatusEventId == statusEventId);
    }

    private static IQueryable<StatusEvent> IncludeTrackingDetails(
        IQueryable<StatusEvent> query) =>
        query
            .Include(e => e.Parcel)
                .ThenInclude(p => p.Route)
                    .ThenInclude(r => r!.OriginHub)
            .Include(e => e.Parcel)
                .ThenInclude(p => p.Route)
                    .ThenInclude(r => r!.DestinationHub)
            .Include(e => e.ParcelStatus)
            .Include(e => e.Hub)
            .Include(e => e.Courier);

    // Get an active hub.
    public async Task<Hub?> GetActiveHubAsync(int hubId)
    {
        return await _context.Hubs
            .FirstOrDefaultAsync(h =>
                h.HubId == hubId &&
                h.IsActive &&
                !h.IsDeleted);
    }

    // Get an active courier.
    public async Task<Courier?> GetActiveCourierAsync(int courierId)
    {
        return await _context.Couriers
            .FirstOrDefaultAsync(c =>
                c.CourierId == courierId &&
                c.IsActive &&
                !c.IsDeleted);
    }

    // Add a new tracking event.
    public async Task AddStatusEventAsync(StatusEvent statusEvent)
    {
        await _context.StatusEvents.AddAsync(statusEvent);
    }

    // Save database changes.
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}