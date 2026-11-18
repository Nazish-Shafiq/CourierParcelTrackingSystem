using CourierParcelTrackingSystem.API.Data;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Extensions;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierParcelTrackingSystem.API.Repositories;

public class HubTransferRepository : IHubTransferRepository
{
    private readonly ApplicationDbContext _context;
    public HubTransferRepository(ApplicationDbContext context) => _context = context;

    public async Task<PagedResponseDto<HubTransfer>> GetPagedAsync(
        PaginationRequestDto request,
        string? search,
        string? status)
    {
        IQueryable<HubTransfer> query = _context.HubTransfers
            .AsNoTracking()
            .Include(t => t.Parcel).ThenInclude(p => p.CurrentStatus)
            .Include(t => t.SourceHub)
            .Include(t => t.DestinationHub)
            .Where(t => !t.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(t =>
                t.Parcel.TrackingNumber.Contains(search) ||
                t.SourceHub.HubName.Contains(search) ||
                t.DestinationHub.HubName.Contains(search));
        }

        return await query
            .OrderByDescending(t => t.TransferredAt)
            .ThenByDescending(t => t.HubTransferId)
            .ToPagedResponseAsync(request);
    }

    public Task<HubTransfer?> GetByIdAsync(long transferId) =>
        _context.HubTransfers
            .Include(t => t.Parcel).ThenInclude(p => p.CurrentStatus)
            .Include(t => t.SourceHub)
            .Include(t => t.DestinationHub)
            .FirstOrDefaultAsync(t => t.HubTransferId == transferId && !t.IsDeleted);

    public Task<Parcel?> GetParcelForTransferAsync(long parcelId) =>
        _context.Parcels
            .Include(p => p.CurrentStatus)
            .Include(p => p.CurrentHub)
            .Include(p => p.Route)
            .Include(p => p.HubTransfers)
            .FirstOrDefaultAsync(p => p.ParcelId == parcelId && p.IsActive && !p.IsDeleted);

    public Task<Hub?> GetActiveHubAsync(int hubId) =>
        _context.Hubs.FirstOrDefaultAsync(h => h.HubId == hubId && h.IsActive && !h.IsDeleted);

    public Task<bool> HasActiveTransferAsync(long parcelId) =>
        _context.HubTransfers.AnyAsync(t =>
            t.ParcelId == parcelId &&
            t.Status == "IN_TRANSIT" &&
            t.IsActive &&
            !t.IsDeleted);

    public Task<ParcelStatus?> GetStatusByCodeAsync(string statusCode) =>
        _context.ParcelStatuses.FirstOrDefaultAsync(s =>
            s.StatusCode == statusCode && s.IsActive && !s.IsDeleted);

    public Task<List<Parcel>> GetEligibleParcelsAsync() =>
        _context.Parcels
            .AsNoTracking()
            .Include(p => p.CurrentStatus)
            .Include(p => p.CurrentHub)
            .Include(p => p.Route)
            .Where(p =>
                p.IsActive &&
                !p.IsDeleted &&
                !p.CurrentStatus.IsTerminal &&
                p.CurrentHubId.HasValue &&
                !p.HubTransfers.Any(t =>
                    t.Status == "IN_TRANSIT" &&
                    t.IsActive &&
                    !t.IsDeleted) &&
                (p.CurrentStatus.StatusCode == "AT_ORIGIN_HUB" ||
                 p.CurrentStatus.StatusCode == "PICKED_UP" ||
                 p.CurrentStatus.StatusCode == "IN_TRANSIT"))
            .OrderBy(p => p.TrackingNumber)
            .ToListAsync();

    public Task<List<Hub>> GetActiveHubsAsync() =>
        _context.Hubs
            .AsNoTracking()
            .Where(h => h.IsActive && !h.IsDeleted)
            .OrderBy(h => h.HubName)
            .ToListAsync();

    public Task AddStatusEventAsync(StatusEvent statusEvent) =>
        _context.StatusEvents.AddAsync(statusEvent).AsTask();

    public Task AddAuditLogAsync(AuditLog auditLog) =>
        _context.AuditLogs.AddAsync(auditLog).AsTask();

    public Task AddAsync(HubTransfer transfer) =>
        _context.HubTransfers.AddAsync(transfer).AsTask();

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
