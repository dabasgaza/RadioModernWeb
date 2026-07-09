using DataAccess.DTOs;
using DataAccess.Services;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Radio.Web.ViewModels;

namespace Radio.Web.Controllers;

[Authorize]
public class ProductionController(IDbContextFactory<BroadcastWorkflowDBContext> ctxFactory) : Controller
{
    public async Task<IActionResult> Index()
    {
        using var context = await ctxFactory.CreateDbContextAsync(HttpContext.RequestAborted);

        var episodes = await context.Episodes
            .AsNoTracking()
            .AsSplitQuery()
            .Where(e => e.IsActive)
            .OrderByDescending(e => e.ScheduledExecutionTime)
            .Select(e => new
            {
                e.EpisodeId,
                e.EpisodeName,
                ProgramName = e.Program != null ? e.Program.ProgramName : null,
                StatusId = e.StatusId,
                StatusDisplay = e.EpisodeStatus != null ? e.EpisodeStatus.DisplayName : "",
                e.ScheduledExecutionTime,
                Guests = e.EpisodeGuests.Select(g => g.Guest.FullName).Take(3).ToList()
            })
            .ToListAsync(HttpContext.RequestAborted);

        var grouped = episodes
            .GroupBy(e => e.StatusId)
            .ToDictionary(g => g.Key.ToString(), g => g.Select(e => new ProductionCard
            {
                EpisodeId = e.EpisodeId,
                EpisodeName = e.EpisodeName,
                ProgramName = e.ProgramName ?? "—",
                StatusDisplay = e.StatusDisplay,
                ScheduledTime = e.ScheduledExecutionTime,
                GuestNames = string.Join("، ", e.Guests)
            }).ToList());

        var vm = new ProductionBoardViewModel
        {
            Columns = new()
            {
                ["1"] = new() { Title = "مجدولة", Cards = grouped.GetValueOrDefault("1", new()) },
                ["2"] = new() { Title = "منفّذة", Cards = grouped.GetValueOrDefault("2", new()) },
                ["3"] = new() { Title = "منشورة", Cards = grouped.GetValueOrDefault("3", new()) },
                ["4"] = new() { Title = "ملغاة", Cards = grouped.GetValueOrDefault("4", new()) },
                ["5"] = new() { Title = "على الموقع", Cards = grouped.GetValueOrDefault("5", new()) }
            }
        };

        return View(vm);
    }
}
