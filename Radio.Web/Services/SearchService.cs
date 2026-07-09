using DataAccess.Services;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Radio.Web.ViewModels;

namespace Radio.Web.Services;

public class SearchService(IDbContextFactory<BroadcastWorkflowDBContext> contextFactory) : ISearchService
{
    public async Task<SearchViewModel> SearchAsync(string query, int maxPerCategory = 10, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new SearchViewModel();

        using var context = await contextFactory.CreateDbContextAsync(ct);
        var q = query.Trim().ToLowerInvariant();

        var episodes = await context.Episodes
            .AsNoTracking()
            .Where(e => e.IsActive && (EF.Functions.Like(e.EpisodeName, $"%{q}%") || EF.Functions.Like(e.Program.ProgramName, $"%{q}%")))
            .OrderByDescending(e => e.ScheduledExecutionTime)
            .Take(maxPerCategory)
            .Select(e => new SearchEpisodeItem
            {
                EpisodeId = e.EpisodeId,
                EpisodeName = e.EpisodeName,
                ProgramName = e.Program != null ? e.Program.ProgramName : null,
                ScheduledTime = e.ScheduledExecutionTime,
                StatusText = e.EpisodeStatus != null ? e.EpisodeStatus.DisplayName : ""
            })
            .ToListAsync(ct);

        var programs = await context.Programs
            .AsNoTracking()
            .Where(p => p.IsActive && (EF.Functions.Like(p.ProgramName, $"%{q}%") || EF.Functions.Like(p.Category, $"%{q}%")))
            .OrderBy(p => p.ProgramName)
            .Take(maxPerCategory)
            .Select(p => new SearchProgramItem
            {
                ProgramId = p.ProgramId,
                ProgramName = p.ProgramName,
                Category = p.Category,
                EpisodeCount = p.Episodes.Count(e => e.IsActive)
            })
            .ToListAsync(ct);

        var guests = await context.Guests
            .AsNoTracking()
            .Where(g => g.IsActive && (EF.Functions.Like(g.FullName, $"%{q}%") || EF.Functions.Like(g.Organization, $"%{q}%")))
            .OrderBy(g => g.FullName)
            .Take(maxPerCategory)
            .Select(g => new SearchGuestItem
            {
                GuestId = g.GuestId,
                FullName = g.FullName,
                Organization = g.Organization,
                Phone = g.PhoneNumber,
                Email = g.EmailAddress
            })
            .ToListAsync(ct);

        return new SearchViewModel
        {
            Query = query,
            Episodes = episodes,
            Programs = programs,
            Guests = guests
        };
    }
}
