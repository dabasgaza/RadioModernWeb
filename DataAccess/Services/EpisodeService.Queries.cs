// ============================================================
// EpisodeService.Queries — استعلامات الحلقات
// ============================================================
// المسؤولية: تعريف استعلامات الحلقات.
// ============================================================
using DataAccess.DTOs;
using Domain.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Services;

/// <summary>
/// صنف الحلقات.
/// </summary>
public partial class EpisodeService
{
    private static readonly Func<BroadcastWorkflowDBContext, IAsyncEnumerable<ActiveEpisodeDto>> s_compiledActiveEpisodes =
        EF.CompileAsyncQuery((BroadcastWorkflowDBContext context) =>
            context.Episodes
                .AsNoTracking()
                .Where(e => e.IsActive)
                .AsSplitQuery()
                .OrderByDescending(e => e.ScheduledExecutionTime.HasValue ? e.ScheduledExecutionTime.Value.Date : DateTime.MinValue)
                .ThenBy(e => e.ScheduledExecutionTime)
                .Select(e => new ActiveEpisodeDto
                {
                    EpisodeId = e.EpisodeId,
                    StatusId = e.StatusId,
                    ProgramId = e.ProgramId,
                    EpisodeName = e.EpisodeName,
                    EpisodeDescription = e.EpisodeDescription,
                    ProgramName = e.Program != null ? e.Program.ProgramName : null,
                    StatusText = e.EpisodeStatus != null ? e.EpisodeStatus.DisplayName : null,
                    ScheduledExecutionTime = e.ScheduledExecutionTime,
                    ActualExecutionTime = e.ActualExecutionTime,
                    SpecialNotes = e.SpecialNotes,
                    CancellationReason = e.CancellationReason,
                    GuestItems = e.EpisodeGuests
                        .OrderBy(g => g.HostingTime)
                        .Select(g => new GuestDisplayItem(g.Guest.GuestId, g.Guest.FullName, g.Topic, g.HostingTime))
                        .ToList(),
                    CorrespondentItems = e.EpisodeCorrespondents
                        .Select(c => new EpisodeCorrespondentDto(
                            c.EpisodeCorrespondentId,
                            c.CorrespondentId,
                            c.Correspondent.FullName,
                            c.Topic,
                            c.HostingTime))
                        .ToList(),
                    EmployeeItems = e.EpisodeEmployees
                        .Select(ee => new EpisodeEmployeeDto(
                            ee.EpisodeEmployeeId,
                            ee.EmployeeId,
                            ee.Employee.FullName,
                            ee.Employee.StaffRole != null ? ee.Employee.StaffRole.RoleName : null))
                        .ToList(),
                }));

    private static readonly Func<BroadcastWorkflowDBContext, int, IAsyncEnumerable<ActiveEpisodeDto>> s_compiledActiveEpisodeById =
        EF.CompileAsyncQuery((BroadcastWorkflowDBContext context, int episodeId) =>
            context.Episodes
                .AsNoTracking()
                .Where(e => e.IsActive && e.EpisodeId == episodeId)
                .AsSplitQuery()
                .Select(e => new ActiveEpisodeDto
                {
                    EpisodeId = e.EpisodeId,
                    StatusId = e.StatusId,
                    ProgramId = e.ProgramId,
                    EpisodeName = e.EpisodeName,
                    EpisodeDescription = e.EpisodeDescription,
                    ProgramName = e.Program != null ? e.Program.ProgramName : null,
                    StatusText = e.EpisodeStatus != null ? e.EpisodeStatus.DisplayName : null,
                    ScheduledExecutionTime = e.ScheduledExecutionTime,
                    ActualExecutionTime = e.ActualExecutionTime,
                    SpecialNotes = e.SpecialNotes,
                    CancellationReason = e.CancellationReason,
                    GuestItems = e.EpisodeGuests
                        .OrderBy(g => g.HostingTime)
                        .Select(g => new GuestDisplayItem(g.Guest.GuestId, g.Guest.FullName, g.Topic, g.HostingTime))
                        .ToList(),
                    CorrespondentItems = e.EpisodeCorrespondents
                        .Select(c => new EpisodeCorrespondentDto(
                            c.EpisodeCorrespondentId,
                            c.CorrespondentId,
                            c.Correspondent.FullName,
                            c.Topic,
                            c.HostingTime))
                        .ToList(),
                    EmployeeItems = e.EpisodeEmployees
                        .Select(ee => new EpisodeEmployeeDto(
                            ee.EpisodeEmployeeId,
                            ee.EmployeeId,
                            ee.Employee.FullName,
                            ee.Employee.StaffRole != null ? ee.Employee.StaffRole.RoleName : null))
                        .ToList(),
                }));

    /// <summary>
    /// Set الضيوف Display.
    /// </summary>
    private static void SetGuestsDisplay(ActiveEpisodeDto episode)
    {
        episode.GuestsDisplay = string.Join(" · ", episode.GuestItems.Select(g => g.Name));
    }

    /// <summary>
    /// استرجاع النشط الحلقات Async.
    /// </summary>
    public async Task<List<ActiveEpisodeDto>> GetActiveEpisodesAsync(CancellationToken cancellationToken = default)
    {
        var operation = _telemetryClient.StartOperation<RequestTelemetry>("GetActiveEpisodes");
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var episodes = new List<ActiveEpisodeDto>();
            await foreach (var ep in s_compiledActiveEpisodes(context))
            {
                SetGuestsDisplay(ep);
                episodes.Add(ep);
            }

            _telemetryClient.TrackMetric("ActiveEpisodesCount", episodes.Count);
            operation.Telemetry.Success = true;
            return episodes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during processing");
            _telemetryClient.TrackException(ex);
            operation.Telemetry.Success = false;
            throw;
        }
        finally
        {
            operation.Dispose();
        }
    }

    /// <summary>
    /// استرجاع النشط الحلقة By Id Async.
    /// </summary>
    public async Task<ActiveEpisodeDto?> GetActiveEpisodeByIdAsync(int episodeId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        ActiveEpisodeDto? episode = null;
        await foreach (var ep in s_compiledActiveEpisodeById(context, episodeId))
        {
            episode = ep;
            break;
        }

        if (episode is null)
            return null;

        SetGuestsDisplay(episode);
        return episode;
    }

    /// <summary>
    /// استرجاع الحلقة الضيوف Async.
    /// </summary>
    public async Task<List<EpisodeGuestDto>> GetEpisodeGuestsAsync(int episodeId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.EpisodeGuests
            .AsNoTracking()
            .Where(eg => eg.EpisodeId == episodeId)
            .OrderBy(eg => eg.HostingTime)
            .Select(eg => new EpisodeGuestDto(
                eg.EpisodeGuestId,
                eg.GuestId,
                eg.Guest.FullName,
                eg.Topic,
                eg.HostingTime,
                eg.ClipNotes))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// استرجاع Conflicting الحلقات Async.
    /// </summary>
    public async Task<List<ConflictInfo>> GetConflictingEpisodesAsync(int programId, DateTime scheduledTime, int? excludeEpisodeId = null, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var windowStart = scheduledTime.AddHours(-1);
        var windowEnd = scheduledTime.AddHours(1);

        return await context.Episodes
            .AsNoTracking()
            .Where(e => e.IsActive
                     && e.StatusId != EpisodeStatusValues.Cancelled
                     && e.EpisodeId != (excludeEpisodeId ?? -1)
                     && e.ScheduledExecutionTime.HasValue
                     && e.ScheduledExecutionTime.Value > windowStart
                     && e.ScheduledExecutionTime.Value < windowEnd)
            .Select(e => new ConflictInfo(
                e.EpisodeId,
                e.EpisodeName ?? string.Empty,
                e.Program != null ? e.Program.ProgramName : string.Empty,
                e.ScheduledExecutionTime!.Value,
                e.ProgramId == programId ? ConflictLevel.High : ConflictLevel.Medium))
            .ToListAsync(cancellationToken);
    }
}
