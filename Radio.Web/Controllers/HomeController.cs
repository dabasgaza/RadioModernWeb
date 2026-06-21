using System.Diagnostics;
using System.Threading;
using DataAccess.Common;
using DataAccess.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Radio.Web.ViewModels;

namespace Radio.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IReportsService _reports;
    private readonly IEpisodeQueryService _episodeQuery;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IReportsService reports,
        IEpisodeQueryService episodeQuery,
        ILogger<HomeController> logger)
    {
        _reports = reports;
        _episodeQuery = episodeQuery;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var todayEpisodes = await _reports.GetTodayEpisodesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
            var statusStats = await _reports.GetEpisodeStatusStatsAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
            var topPrograms = await _reports.GetMostActiveProgramsAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
            var topGuests = await _reports.GetTopGuestsAsync(10, cancellationToken: HttpContext?.RequestAborted ?? default);
            var cancelledEpisodes = await _reports.GetCancelledEpisodesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);

            var vm = new DashboardViewModel
            {
                TodayEpisodes = todayEpisodes,
                StatusStats = statusStats,
                TopPrograms = topPrograms,
                TopGuests = topGuests,
                CancelledEpisodes = cancelledEpisodes,
                TotalEpisodes = statusStats.Values.Sum(),
                TotalPrograms = topPrograms.Count
            };
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "فشل في تحميل لوحة التحكم");
            return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
