using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Radio.Web.Services;

namespace Radio.Web.Controllers;

[Authorize]
public class SearchController(ISearchService search) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q))
            return View(new Radio.Web.ViewModels.SearchViewModel());

        var results = await search.SearchAsync(q, ct: ct);
        return View(results);
    }
}
