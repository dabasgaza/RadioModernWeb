// ============================================================
// PagedResult — النتائج المقسمة
// ============================================================
// المسؤولية: تعريف النتائج المقسمة.
// ============================================================
namespace DataAccess.Common;

/// <summary>
/// النتائج المقسمة: صنف النتائج المقسمة.
/// <summary>
/// صنف النتائج المقسمة.
/// </summary>
/// <summary>
/// صنف النتائج المقسمة.
/// </summary>
/// <summary>
/// صنف النتائج المقسمة.
/// </summary>
/// <summary>
/// صنف النتائج المقسمة.
/// </summary>
/// <summary>
/// صنف النتائج المقسمة.
/// </summary>
/// <summary>
/// صنف النتائج المقسمة.
/// </summary>
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; init; } = [];
    public int PageIndex { get; init; }
    public int TotalPages { get; init; }
    public int TotalCount { get; init; }
    public int PageSize { get; init; }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
}
