using Radio.Web.ViewModels;

namespace Radio.Web.Services;

public interface ISearchService
{
    Task<SearchViewModel> SearchAsync(string query, int maxPerCategory = 10, CancellationToken ct = default);
}
