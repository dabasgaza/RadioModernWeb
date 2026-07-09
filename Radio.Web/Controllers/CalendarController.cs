using DataAccess.Services;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Radio.Web.Controllers;

[Authorize]
public class CalendarController(IDbContextFactory<BroadcastWorkflowDBContext> ctxFactory) : Controller
{
    public IActionResult Index()
    {
        var now = DateTime.UtcNow;
        ViewBag.Year = now.Year;
        ViewBag.Month = now.Month;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents(int year, int month)
    {
        using var context = await ctxFactory.CreateDbContextAsync(HttpContext.RequestAborted);
        var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddMonths(1);

        var episodes = await context.Episodes
            .AsNoTracking()
            .Where(e => e.IsActive && e.ScheduledExecutionTime >= from && e.ScheduledExecutionTime < to)
            .OrderBy(e => e.ScheduledExecutionTime)
            .Select(e => new
            {
                e.EpisodeId,
                e.EpisodeName,
                e.ScheduledExecutionTime,
                Status = e.EpisodeStatus != null ? e.EpisodeStatus.DisplayName : "",
                StatusCss = e.StatusId == 1 ? "planned" :
                            e.StatusId == 2 ? "executed" :
                            e.StatusId == 3 ? "published" :
                            e.StatusId == 4 ? "cancelled" :
                            e.StatusId == 5 ? "website-published" : "planned"
            })
            .ToListAsync(HttpContext.RequestAborted);

        var events = episodes.Select(e => new
        {
            id = e.EpisodeId,
            title = e.EpisodeName,
            start = e.ScheduledExecutionTime?.ToString("yyyy-MM-ddTHH:mm:ss"),
            statusCss = e.StatusCss,
            url = $"/Episodes/Details/{e.EpisodeId}",
            status = e.Status,
            allDay = false
        });

        return Json(new { events, month, year });
    }
}
