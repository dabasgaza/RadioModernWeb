// ============================================================
// PagedListViewModel — ViewModel القائمة
// ============================================================
// المسؤولية: تعريف ViewModel القائمة.
// ============================================================
namespace Radio.Web.ViewModels;

/// <summary>
/// القائمة المقسمة: صنف القائمة المقسمة.
/// <summary>
/// صنف ViewModel القائمة.
/// </summary>
/// <summary>
/// صنف ViewModel القائمة.
/// </summary>
/// <summary>
/// صنف ViewModel القائمة.
/// </summary>
/// <summary>
/// صنف ViewModel القائمة.
/// </summary>
/// <summary>
/// صنف ViewModel القائمة.
/// </summary>
/// <summary>
/// صنف ViewModel القائمة.
/// </summary>
/// </summary>
public class PagedListViewModel<T>
{
    public List<T> Items { get; set; } = [];
    public int PageIndex { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
}
