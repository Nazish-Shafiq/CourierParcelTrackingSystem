using CourierParcelTrackingSystem.API.Data;
using CourierParcelTrackingSystem.API.DTOs.AuditLogDTO;
using CourierParcelTrackingSystem.API.DTOs.Common;
using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierParcelTrackingSystem.API.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context) => _context = context;

    public async Task<PagedResponseDto<AuditLog>> GetPagedAsync(
        AuditLogQueryDto request)
    {
        IQueryable<AuditLog> query = _context.AuditLogs
            .AsNoTracking()
            .Include(log => log.ActorUser);

        if (!string.IsNullOrWhiteSpace(request.EntityName))
        {
            var entityName = request.EntityName.Trim();
            query = query.Where(log => log.EntityName.Contains(entityName));
        }

        if (!string.IsNullOrWhiteSpace(request.EntityId))
        {
            var entityId = request.EntityId.Trim();
            query = query.Where(log => log.EntityId.Contains(entityId));
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            var action = request.Action.Trim();
            query = query.Where(log => log.Action == action);
        }

        if (request.ActorUserId.HasValue)
            query = query.Where(log => log.ActorUserId == request.ActorUserId);

        if (request.FromDate.HasValue)
            query = query.Where(log => log.OccurredAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(log => log.OccurredAt <= request.ToDate.Value);

        var totalRecords = await query.CountAsync();
        var orderedQuery = request.SortAscending
            ? query.OrderBy(log => log.OccurredAt).ThenBy(log => log.AuditLogId)
            : query.OrderByDescending(log => log.OccurredAt)
                .ThenByDescending(log => log.AuditLogId);
        var items = await orderedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResponseDto<AuditLog>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(
                (double)totalRecords / request.PageSize)
        };
    }

    public async Task AddAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }
}
