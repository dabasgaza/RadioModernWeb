// ============================================================
// QueryableExtensions — الاستعلامات
// ============================================================
// المسؤولية: تعريف الاستعلامات.
// ============================================================
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Common;

/// <summary>
/// الاستعلامات: صنف الاستعلامات.
/// <summary>
/// صنف الاستعلامات.
/// </summary>
/// <summary>
/// صنف الاستعلامات.
/// </summary>
/// <summary>
/// صنف الاستعلامات.
/// </summary>
/// <summary>
/// صنف الاستعلامات.
/// </summary>
/// <summary>
/// صنف الاستعلامات.
/// </summary>
/// <summary>
/// صنف الاستعلامات.
/// </summary>
/// </summary>
public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> source, int pageIndex, int pageSize, CancellationToken ct = default)
    {
        var totalCount = await source.CountAsync(ct);
        var items = await source
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>
        {
            Items = items,
            PageIndex = pageIndex,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            TotalCount = totalCount,
            PageSize = pageSize
        };
    }
}
