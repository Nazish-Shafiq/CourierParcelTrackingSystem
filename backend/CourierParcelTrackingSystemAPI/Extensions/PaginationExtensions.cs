
using Microsoft.EntityFrameworkCore;
using CourierParcelTrackingSystem.API.DTOs.Common;

namespace CourierParcelTrackingSystem.API.Extensions;

public static class PaginationExtensions
{
    public static async Task<PagedResponseDto<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query,
        PaginationRequestDto pagination)
    {
        var totalRecords = await query.CountAsync();

        var totalPages = (int)Math.Ceiling(
            totalRecords / (double)pagination.PageSize);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedResponseDto<T>
        {
            Items = items,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }
}
