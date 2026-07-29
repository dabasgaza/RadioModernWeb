// ============================================================
// HomeController — الصفحة الرئيسية
// ============================================================
// المسؤولية: تعريف الصفحة الرئيسية.
// ============================================================
using DataAccess.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Radio.Web.ViewModels;
using System.Diagnostics;

namespace Radio.Web.Controllers;

/// <summary>
/// صنف الصفحة الرئيسية.
/// </summary>
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

    /// <summary>
    /// عرض قائمة الصفحة الرئيسية.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var todayEpisodes = await _reports.GetTodayEpisodesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
            var statusStats = await _reports.GetEpisodeStatusStatsAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
            var topPrograms = await _reports.GetMostActiveProgramsAsync(cancellationToken: HttpContext?.RequestAborted ?? default);
            var topGuests = await _reports.GetTopGuestsAsync(10, cancellationToken: HttpContext?.RequestAborted ?? default);
            var cancelledEpisodes = await _reports.GetCancelledEpisodesAsync(cancellationToken: HttpContext?.RequestAborted ?? default);

            var totalEpisodes = statusStats.Values.Sum();
            var totalPrograms = topPrograms.Count;

            var vm = new DashboardViewModel
            {
                TodayEpisodes = todayEpisodes,
                StatusStats = statusStats,
                TopPrograms = topPrograms,
                TopGuests = topGuests,
                CancelledEpisodes = cancelledEpisodes,
                TotalEpisodes = totalEpisodes,
                TotalPrograms = totalPrograms,
                KpiItems =
                [
                    new() { Label = "حلقات البث اليوم", Value = todayEpisodes.Count.ToString(), Subtitle = "مجدولة لليوم", Icon = "live_tv", Color = "var(--color-primary)" },
                    new() { Label = "إجمالي الحلقات", Value = totalEpisodes.ToString(), Subtitle = "الأرشيف والتنفيذ", Icon = "analytics", Color = "var(--color-accent)" },
                    new() { Label = "البرامج النشطة", Value = totalPrograms.ToString(), Subtitle = "الدورة البرامجية", Icon = "tv", Color = "var(--color-warning)" },
                    new() { Label = "حلقات ملغاة", Value = cancelledEpisodes.Count.ToString(), Subtitle = "موقوفة أو مستبعدة", Icon = "cancel", Color = "var(--color-danger)" }
                ]
            };
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "فشل في تحميل لوحة التحكم");
            return View("Error", new ErrorViewModel { ErrorMessage = ex.Message });
        }
    }

    /// <summary>
    /// Error.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
