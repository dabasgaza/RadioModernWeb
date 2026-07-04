// ============================================================
// ReportsService — التقارير
// ============================================================
// المسؤولية: تعريف التقارير.
// ============================================================
using DataAccess.DTOs;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Services;

/// <summary>
/// واجهة I التقارير.
/// </summary>
public interface IReportsService
{
    Task<List<TodayEpisodeDto>> GetTodayEpisodesAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetEpisodeStatusStatsAsync(CancellationToken cancellationToken = default);
    Task<List<ActiveProgramDto>> GetMostActiveProgramsAsync(CancellationToken cancellationToken = default);
    Task<List<DateRangeEpisodeDto>> GetEpisodesByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<List<TopGuestDto>> GetTopGuestsAsync(int topCount = 10, CancellationToken cancellationToken = default);
    Task<List<CancelledEpisodeDto>> GetCancelledEpisodesAsync(CancellationToken cancellationToken = default);
}

// ✨ استخدام Primary Constructor
/// <summary>
/// صنف التقارير.
/// </summary>
public class ReportsService(IDbContextFactory<BroadcastWorkflowDBContext> contextFactory) : IReportsService
{
    /// <summary>
    /// استرجاع الحلقة الحالة Stats Async.
    /// </summary>
    public async Task<Dictionary<string, int>> GetEpisodeStatusStatsAsync(CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync();

        var stats = await context.Episodes.AsNoTracking()
            .Where(e => e.IsActive)
            .GroupBy(e => e.EpisodeStatus.StatusName)
            .Select(g => new { StatusName = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return stats.ToDictionary(x => x.StatusName, x => x.Count);
    }

    /// <summary>
    /// استرجاع Today الحلقات Async.
    /// </summary>
    public async Task<List<TodayEpisodeDto>> GetTodayEpisodesAsync(CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync();

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        // ✅ Select مباشر + AsSplitQuery: نجلب الحقول المطلوبة فقط بدلاً من الكيانات كاملة
        var raw = await context.Episodes
            .AsNoTracking()
            .AsSplitQuery()
            .Where(e => e.IsActive && e.ScheduledExecutionTime >= today && e.ScheduledExecutionTime < tomorrow)
            .OrderBy(e => e.ScheduledExecutionTime)
            .Select(e => new
            {
                e.EpisodeId,
                e.EpisodeName,
                ProgramName = e.Program != null ? e.Program.ProgramName : null,
                e.ScheduledExecutionTime,
                StatusDisplayName = e.EpisodeStatus != null ? e.EpisodeStatus.DisplayName : null,
                Guests = e.EpisodeGuests
                    .OrderBy(g => g.HostingTime)
                    .Select(g => new GuestDisplayItem(g.Guest.GuestId, g.Guest.FullName, g.Topic, g.HostingTime))
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        // ✅ تحويل النتيجة إلى DTOs في الذاكرة مع تنسيق أسماء الضيوف
        return raw.Select(e => new TodayEpisodeDto(
            e.EpisodeId,
            e.EpisodeName,
            e.ProgramName ?? "—",
            FormatGuestItemsDisplay(e.Guests),
            e.ScheduledExecutionTime,
            e.StatusDisplayName ?? "غير معروف"
        )).ToList();
    }

    /// <summary>
    /// استرجاع Most Active البرامج Async.
    /// </summary>
    public async Task<List<ActiveProgramDto>> GetMostActiveProgramsAsync(CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync();

        return await context.Programs
            .AsNoTracking()
            .Select(p => new ActiveProgramDto
            {
                ProgramName = p.ProgramName,
                Category = p.Category,
                TotalEpisodes = p.Episodes.Count(),
                // ✨ استخدام الثوابت بدلاً من الأرقام السحرية
                PublishedEpisodes = p.Episodes.Count(e => e.IsActive && e.StatusId == EpisodeStatusValues.Published)
            })
            .OrderByDescending(x => x.TotalEpisodes)
            .Take(5)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// استرجاع الحلقات By التاريخ Range Async.
    /// </summary>
    public async Task<List<DateRangeEpisodeDto>> GetEpisodesByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync();

        var toEndOfDay = to.Date.AddDays(1);

        // ✅ Select مباشر + AsSplitQuery: نجلب الحقول المطلوبة فقط
        var raw = await context.Episodes
            .AsNoTracking()
            .AsSplitQuery()
            .Where(e => e.IsActive && e.ScheduledExecutionTime >= from.Date && e.ScheduledExecutionTime < toEndOfDay)
            .OrderBy(e => e.ScheduledExecutionTime)
            .Select(e => new
            {
                e.EpisodeId,
                e.EpisodeName,
                ProgramName = e.Program != null ? e.Program.ProgramName : null,
                e.ScheduledExecutionTime,
                StatusDisplayName = e.EpisodeStatus != null ? e.EpisodeStatus.DisplayName : null,
                Guests = e.EpisodeGuests
                    .OrderBy(g => g.HostingTime)
                    .Select(g => new GuestDisplayItem(g.Guest.GuestId, g.Guest.FullName, g.Topic, g.HostingTime))
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        // ✅ تحويل النتيجة إلى DTOs في الذاكرة مع تنسيق أسماء الضيوف
        return raw.Select(e => new DateRangeEpisodeDto(
            e.EpisodeId,
            e.EpisodeName,
            e.ProgramName ?? "—",
            FormatGuestItemsDisplay(e.Guests),
            e.ScheduledExecutionTime,
            e.StatusDisplayName ?? "غير معروف"
        )).ToList();
    }

    /// <summary>
    /// استرجاع Top الضيوف Async.
    /// </summary>
    public async Task<List<TopGuestDto>> GetTopGuestsAsync(int topCount = 10, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync();

        // ── تنفيذ GROUP BY في قاعدة البيانات بدلاً من تحميل الكل في الذاكرة ──
        var grouped = await context.EpisodeGuests
            .AsNoTracking()
            .GroupBy(eg => eg.GuestId)
            .Select(g => new
            {
                GuestId = g.Key,
                AppearanceCount = g.Count(),
                LastEpisodeGuestId = g
                    .OrderByDescending(eg => eg.Episode.ScheduledExecutionTime)
                    .Select(eg => eg.EpisodeGuestId)
                    .FirstOrDefault()
            })
            .OrderByDescending(x => x.AppearanceCount)
            .Take(topCount)
            .ToListAsync(cancellationToken);

        // ── جلب تفاصيل آخر ظهور فقط للضيوف المطلوبين ──
        var lastGuestIds = grouped.Select(x => x.LastEpisodeGuestId).ToList();
        var lastDetails = await context.EpisodeGuests
            .AsNoTracking()
            .Where(eg => lastGuestIds.Contains(eg.EpisodeGuestId))
            .Select(eg => new
            {
                eg.EpisodeGuestId,
                GuestFullName = eg.Guest != null ? eg.Guest.FullName : null,
                GuestOrganization = eg.Guest != null ? eg.Guest.Organization : null,
                eg.Topic,
                EpisodeScheduledTime = eg.Episode != null ? eg.Episode.ScheduledExecutionTime : (DateTime?)null
            })
            .ToDictionaryAsync(eg => eg.EpisodeGuestId, cancellationToken);

        return grouped.Select((x, i) =>
        {
            lastDetails.TryGetValue(x.LastEpisodeGuestId, out var last);
            return new TopGuestDto(
                i + 1,
                x.GuestId,
                last?.GuestFullName ?? "غير معروف",
                last?.GuestOrganization,
                x.AppearanceCount,
                last?.Topic,
                last?.EpisodeScheduledTime
            );
        }).ToList();
    }

    /// <summary>
    /// استرجاع Cancelled الحلقات Async.
    /// </summary>
    public async Task<List<CancelledEpisodeDto>> GetCancelledEpisodesAsync(CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync();

        // ✅ Select مباشر بدلاً من Include لتجنب جلب كل أعمدة Program
        var episodes = await context.Episodes
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(e => e.StatusId == EpisodeStatusValues.Cancelled)
            .OrderByDescending(e => e.UpdatedAt)
            .Select(e => new
            {
                e.EpisodeId,
                e.EpisodeName,
                ProgramName = e.Program != null ? e.Program.ProgramName : null,
                e.ScheduledExecutionTime,
                e.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        if (episodes.Count == 0)
            return new List<CancelledEpisodeDto>();

        var episodeIds = episodes.Select(e => e.EpisodeId).ToList();

        // جلب سجلات الإلغاء من AuditLog
        var auditLogs = await context.AuditLogs
            .AsNoTracking()
            .Where(a => a.TableName == "Episodes"
                     && a.Action == "CANCEL"
                     && a.RecordId != null
                     && episodeIds.Contains(a.RecordId.Value))
            .OrderByDescending(a => a.ChangedAt)
            .ToListAsync(cancellationToken);

        // جلب أسماء المستخدمين الذين ألغوا
        var userIds = auditLogs.Where(a => a.UserId.HasValue).Select(a => a.UserId!.Value).Distinct().ToList();
        var users = await context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        // بناء قاموس آخر سجل إلغاء لكل حلقة
        var logDict = auditLogs
            .GroupBy(a => a.RecordId!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        return episodes.Select(e =>
        {
            logDict.TryGetValue(e.EpisodeId, out var log);
            var cancelledBy = log?.UserId.HasValue == true
                ? users.GetValueOrDefault(log.UserId!.Value, "غير معروف")
                : "غير معروف";

            return new CancelledEpisodeDto(
                e.EpisodeId,
                e.EpisodeName,
                e.ProgramName ?? "—",
                e.ScheduledExecutionTime,
                log?.Reason ?? "لم يتم تحديد سبب",
                cancelledBy,
                log?.ChangedAt ?? e.UpdatedAt
            );
        }).ToList();
    }

    /// <summary>
    /// تنسيق أسماء الضيوف وعناوينهم ومواعيدهم من قائمة GuestDisplayItem
    /// <summary>
    /// تنسيق الضيف Items Display.
    /// </summary>
    /// </summary>
    private static string FormatGuestItemsDisplay(IEnumerable<GuestDisplayItem> guests)
    {
        var list = guests.ToList();

        if (list.Count == 0)
            return "لا يوجد ضيف";

        return string.Join(" ، ", list.Select(g =>
        {
            var name = g.Name ?? "غير معروف";
            if (g.HostingTime.HasValue)
                name += $" ({g.HostingTime.Value:hh\\:mm})";
            if (!string.IsNullOrWhiteSpace(g.Topic))
                name += $" — {g.Topic}";
            return name;
        }));
    }

    /// <summary>
    /// تنسيق أسماء الضيوف من كيانات EpisodeGuest (للاستخدام الداخلي فقط)
    /// <summary>
    /// تنسيق الضيوف Display.
    /// </summary>
    /// </summary>
    private static string FormatGuestsDisplay(IEnumerable<EpisodeGuest> guests)
    {
        var list = guests.OrderBy(g => g.HostingTime).ToList();

        if (list.Count == 0)
            return "لا يوجد ضيف";

        return string.Join(" ، ", list.Select(g =>
        {
            var name = g.Guest?.FullName ?? "غير معروف";
            if (g.HostingTime.HasValue)
                name += $" ({g.HostingTime.Value:hh\\:mm})";
            if (!string.IsNullOrWhiteSpace(g.Topic))
                name += $" — {g.Topic}";
            return name;
        }));
    }
}