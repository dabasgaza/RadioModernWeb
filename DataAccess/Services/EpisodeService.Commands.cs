// ============================================================
// EpisodeService.Commands — أوامر الحلقات
// ============================================================
// المسؤولية: تعريف أوامر الحلقات.
// ============================================================
using DataAccess.Common;
using DataAccess.DTOs;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DataAccess.Services;

/// <summary>
/// صنف الحلقات.
/// </summary>
public partial class EpisodeService
{
    /// <summary>
    /// إنشاء الحلقة Async.
    /// </summary>
    public async Task<Result<int>> CreateEpisodeAsync(EpisodeDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodeManage);
        if (!permCheck.IsSuccess) return Result<int>.Fail(permCheck.ErrorMessage!);

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var programExists = await context.Programs.AnyAsync(p => p.ProgramId == dto.ProgramId && p.IsActive, cancellationToken);
            if (!programExists) return Result<int>.Fail("البرنامج المحدد غير موجود أو غير نشط.");

            if (dto.Guests?.Count > 0)
            {
                var guestIds = dto.Guests.Select(g => g.GuestId).ToList();
                var existingCount = await context.Guests.CountAsync(g => guestIds.Contains(g.GuestId) && g.IsActive, cancellationToken);
                if (existingCount != guestIds.Distinct().Count())
                    return Result<int>.Fail("بعض الضيوف المحددين غير موجودين أو تم حذفهم.");
            }

            if (dto.Correspondents?.Count > 0)
            {
                var corrIds = dto.Correspondents.Select(c => c.CorrespondentId).ToList();
                var existingCount = await context.Correspondents.CountAsync(c => corrIds.Contains(c.CorrespondentId) && c.IsActive, cancellationToken);
                if (existingCount != corrIds.Distinct().Count())
                    return Result<int>.Fail("بعض المراسلين المحددين غير موجودين أو تم حذفهم.");
            }

            if (dto.Employees?.Count > 0)
            {
                var empIds = dto.Employees.Select(ee => ee.EmployeeId).ToList();
                var existingCount = await context.Employees.CountAsync(e => empIds.Contains(e.EmployeeId) && e.IsActive, cancellationToken);
                if (existingCount != empIds.Distinct().Count())
                    return Result<int>.Fail("بعض الموظفين المحددين غير موجودين في النظام. قد يكون تم حذفهم أو أنك تستخدم مسودة قديمة.");
            }

            var episode = new Episode
            {
                ProgramId = dto.ProgramId,
                EpisodeName = dto.EpisodeName,
                EpisodeDescription = dto.EpisodeDescription,
                ScheduledExecutionTime = dto.ScheduledDateTime,
                StatusId = EpisodeStatusValues.Planned,
                SpecialNotes = dto.SpecialNotes
            };

            if (dto.Guests?.Count > 0)
                foreach (var g in dto.Guests)
                    episode.EpisodeGuests.Add(new EpisodeGuest
                    {
                        GuestId = g.GuestId,
                        Topic = g.Topic,
                        HostingTime = g.HostingTime,
                        ClipNotes = g.ClipNotes
                    });

            if (dto.Correspondents?.Count > 0)
                foreach (var c in dto.Correspondents)
                    episode.EpisodeCorrespondents.Add(new EpisodeCorrespondent
                    {
                        CorrespondentId = c.CorrespondentId,
                        Topic = c.Topic,
                        HostingTime = c.HostingTime
                    });

            if (dto.Employees?.Count > 0)
                foreach (var ee in dto.Employees)
                    episode.EpisodeEmployees.Add(new EpisodeEmployee { EmployeeId = ee.EmployeeId });

            context.Episodes.Add(episode);
            await context.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(episode.EpisodeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Episode: {EpisodeName}, ProgramId: {ProgramId}", dto.EpisodeName, dto.ProgramId);
            return Result<int>.Fail("حدث خطأ في قاعدة البيانات أثناء جدولة الحلقة. يرجى المحاولة لاحقاً.");
        }
    }

    /// <summary>
    /// تحديث الحلقة Async.
    /// </summary>
    public async Task<Result> UpdateEpisodeAsync(EpisodeDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodeEdit);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var programExists = await context.Programs.AnyAsync(p => p.ProgramId == dto.ProgramId && p.IsActive, cancellationToken);
            if (!programExists) return Result.Fail("البرنامج المحدد غير موجود أو غير نشط.");

            if (dto.Guests?.Count > 0)
            {
                var guestIds = dto.Guests.Select(g => g.GuestId).ToList();
                var existingCount = await context.Guests.CountAsync(g => guestIds.Contains(g.GuestId) && g.IsActive, cancellationToken);
                if (existingCount != guestIds.Distinct().Count())
                    return Result.Fail("بعض الضيوف المحددين غير موجودين أو تم حذفهم.");
            }

            if (dto.Correspondents?.Count > 0)
            {
                var corrIds = dto.Correspondents.Select(c => c.CorrespondentId).ToList();
                var existingCount = await context.Correspondents.CountAsync(c => corrIds.Contains(c.CorrespondentId) && c.IsActive, cancellationToken);
                if (existingCount != corrIds.Distinct().Count())
                    return Result.Fail("بعض المراسلين المحددين غير موجودين أو تم حذفهم.");
            }

            if (dto.Employees?.Count > 0)
            {
                var empIds = dto.Employees.Select(ee => ee.EmployeeId).ToList();
                var existingCount = await context.Employees.CountAsync(e => empIds.Contains(e.EmployeeId) && e.IsActive, cancellationToken);
                if (existingCount != empIds.Distinct().Count())
                    return Result.Fail("بعض الموظفين المحددين غير موجودين في النظام. قد يكون تم حذفهم أو أنك تستخدم مسودة قديمة.");
            }

            var episode = await context.Episodes
                .Include(e => e.EpisodeGuests)
                .Include(e => e.EpisodeCorrespondents)
                .Include(e => e.EpisodeEmployees)
                .FirstOrDefaultAsync(e => e.EpisodeId == dto.EpisodeId, cancellationToken);

            if (episode == null) return Result.Fail("الحلقة غير موجودة.");

            var allEpisodeEmployees = await context.EpisodeEmployees
                .IgnoreQueryFilters()
                .Where(ee => ee.EpisodeId == dto.EpisodeId)
                .ToListAsync(cancellationToken);

            var allEpisodeGuests = await context.EpisodeGuests
                .IgnoreQueryFilters()
                .Where(eg => eg.EpisodeId == dto.EpisodeId)
                .ToListAsync(cancellationToken);

            var allEpisodeCorrespondents = await context.EpisodeCorrespondents
                .IgnoreQueryFilters()
                .Where(ec => ec.EpisodeId == dto.EpisodeId)
                .ToListAsync(cancellationToken);

            episode.ProgramId = dto.ProgramId;
            episode.EpisodeName = dto.EpisodeName;
            episode.EpisodeDescription = dto.EpisodeDescription;
            episode.ScheduledExecutionTime = dto.ScheduledDateTime;
            episode.SpecialNotes = dto.SpecialNotes;

            CollectionSyncHelper.Sync(
                episode.EpisodeGuests.ToList(), allEpisodeGuests, dto.Guests ?? [], episode,
                entityIdSelector: g => g.EpisodeGuestId,
                dtoIdSelector: d => d.EpisodeGuestId,
                entityFkSelector: g => g.GuestId,
                dtoFkSelector: d => d.GuestId,
                updater: (g, d) => { g.GuestId = d.GuestId; g.Topic = d.Topic; g.HostingTime = d.HostingTime; g.ClipNotes = d.ClipNotes; },
                factory: d => new EpisodeGuest { GuestId = d.GuestId, Topic = d.Topic, HostingTime = d.HostingTime, ClipNotes = d.ClipNotes },
                addToParent: (ep, g) => ep.EpisodeGuests.Add(g));

            CollectionSyncHelper.Sync(
                episode.EpisodeCorrespondents.ToList(), allEpisodeCorrespondents, dto.Correspondents ?? [], episode,
                entityIdSelector: c => c.EpisodeCorrespondentId,
                dtoIdSelector: d => d.Id,
                entityFkSelector: c => c.CorrespondentId,
                dtoFkSelector: d => d.CorrespondentId,
                updater: (c, d) => { c.CorrespondentId = d.CorrespondentId; c.Topic = d.Topic; c.HostingTime = d.HostingTime; },
                factory: d => new EpisodeCorrespondent { CorrespondentId = d.CorrespondentId, Topic = d.Topic, HostingTime = d.HostingTime },
                addToParent: (ep, c) => ep.EpisodeCorrespondents.Add(c));

            CollectionSyncHelper.Sync(
                episode.EpisodeEmployees.ToList(), allEpisodeEmployees, dto.Employees ?? [], episode,
                entityIdSelector: e => e.EpisodeEmployeeId,
                dtoIdSelector: d => d.Id,
                entityFkSelector: e => e.EmployeeId,
                dtoFkSelector: d => d.EmployeeId,
                updater: (e, d) => { e.EmployeeId = d.EmployeeId; },
                factory: d => new EpisodeEmployee { EmployeeId = d.EmployeeId },
                addToParent: (ep, e) => ep.EpisodeEmployees.Add(e));

            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Episode: {EpisodeId}, {EpisodeName}", dto.EpisodeId, dto.EpisodeName);
            return Result.Fail("حدث خطأ في قاعدة البيانات أثناء تعديل بيانات الحلقة. يرجى المحاولة لاحقاً.");
        }
    }

    /// <summary>
    /// تحديث الحالة Async.
    /// </summary>
    public async Task<Result> UpdateStatusAsync(int episodeId, byte newStatusId, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = newStatusId == EpisodeStatusValues.Executed
            ? session.EnsurePermission(AppPermissions.EpisodeExecute)
            : session.EnsurePermission(AppPermissions.EpisodeManage);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var episode = await context.Episodes.FindAsync(episodeId, cancellationToken);
        if (episode == null) return Result.Fail("الحلقة غير موجودة.");

        if (!IsValidTransition(episode.StatusId, newStatusId))
        {
            var currentStatus = EpisodeStatusValues.GetDisplayName(episode.StatusId);
            var targetStatus = EpisodeStatusValues.GetDisplayName(newStatusId);
            return Result.Fail($"لا يمكن الانتقال من حالة ({currentStatus}) إلى ({targetStatus}). يجب اتباع التسلسل الصحيح للحالات.");
        }

        var oldStatusId = episode.StatusId;
        episode.StatusId = newStatusId;
        if (newStatusId == EpisodeStatusValues.Executed)
            episode.ActualExecutionTime = DateTime.UtcNow;

        AddStatusAuditLog(context, episodeId, oldStatusId, newStatusId, session.UserId, reason: null);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// Revert الحلقة الحالة Async.
    /// </summary>
    public async Task<Result> RevertEpisodeStatusAsync(int episodeId, string reason, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodeRevert);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Fail("يجب إدخال سبب التراجع عن الحالة.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var episode = await context.Episodes.FindAsync(episodeId, cancellationToken);
        if (episode == null) return Result.Fail("الحلقة غير موجودة.");

        var oldStatusId = episode.StatusId;
        byte targetStatusId;

        switch (episode.StatusId)
        {
            case EpisodeStatusValues.Executed:
                var execLog = await context.ExecutionLogs
                    .Where(l => l.EpisodeId == episodeId && l.IsActive)
                    .OrderByDescending(l => l.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);
                if (execLog != null) execLog.IsActive = false;
                episode.StatusId = EpisodeStatusValues.Planned;
                episode.ActualExecutionTime = null;
                targetStatusId = EpisodeStatusValues.Planned;
                break;

            case EpisodeStatusValues.Published:
                var socialLogs = await context.SocialMediaPublishingLogs
                    .Where(l => l.EpisodeGuest.EpisodeId == episodeId && l.IsActive)
                    .ToListAsync(cancellationToken);
                foreach (var log in socialLogs) log.IsActive = false;
                episode.StatusId = EpisodeStatusValues.Executed;
                targetStatusId = EpisodeStatusValues.Executed;
                break;

            case EpisodeStatusValues.WebsitePublished:
                var webLog = await context.WebsitePublishingLogs
                    .Where(l => l.EpisodeId == episodeId && l.IsActive)
                    .OrderByDescending(l => l.PublishedAt)
                    .FirstOrDefaultAsync(cancellationToken);
                if (webLog != null) webLog.IsActive = false;
                episode.StatusId = EpisodeStatusValues.Published;
                targetStatusId = EpisodeStatusValues.Published;
                break;

            case EpisodeStatusValues.Planned:
                return Result.Fail("لا يمكن التراجع عن حلقة في حالة (مجدولة) — هي بالفعل في أول مرحلة.");

            case EpisodeStatusValues.Cancelled:
                return Result.Fail("لا يمكن التراجع عن حلقة ملغاة. استخدم إعادة الجدولة بدلاً من ذلك.");

            default:
                return Result.Fail($"حالة الحلقة غير معروفة ({episode.StatusId}).");
        }

        AddStatusAuditLog(context, episodeId, oldStatusId, targetStatusId, session.UserId, reason);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// إلغاء الحلقة Async.
    /// </summary>
    public async Task<Result> CancelEpisodeAsync(int episodeId, string reason, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodeManage);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Fail("يجب إدخال سبب إلغاء الحلقة.");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var episode = await context.Episodes.FindAsync(episodeId, cancellationToken);
        if (episode == null) return Result.Fail("الحلقة غير موجودة.");

        if (episode.StatusId == EpisodeStatusValues.Cancelled)
            return Result.Fail("الحلقة ملغاة بالفعل.");

        var oldStatusId = episode.StatusId;
        episode.StatusId = EpisodeStatusValues.Cancelled;
        episode.CancellationReason = reason;

        AddStatusAuditLog(context, episodeId, oldStatusId, EpisodeStatusValues.Cancelled, session.UserId, reason);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// تحديث Cancellation Reason Async.
    /// </summary>
    public async Task<Result> UpdateCancellationReasonAsync(int episodeId, string newReason, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodeEdit);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var episode = await context.Episodes.FindAsync(episodeId, cancellationToken);
        if (episode == null) return Result.Fail("الحلقة غير موجودة.");

        if (episode.StatusId != EpisodeStatusValues.Cancelled)
            return Result.Fail("لا يمكن تعديل سبب الإلغاء لحلقة ليست في حالة ملغاة.");

        episode.CancellationReason = newReason;
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// تبديل Website Publish Async.
    /// </summary>
    public async Task<Result> ToggleWebsitePublishAsync(int episodeId, bool isPublished, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodeWebPublish);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        await using var context = await _contextFactory.CreateDbContextAsync();

        if (!await context.EnsureDomainUserExistsAsync(session))
            return Result.Fail("المستخدم غير موجود في النظام. الرجاء تسجيل الخروج وإعادة تسجيل الدخول.");

        var episode = await context.Episodes.FindAsync(episodeId, cancellationToken);
        if (episode == null) return Result.Fail("الحلقة غير موجودة.");

        if (isPublished)
        {
            context.WebsitePublishingLogs.Add(new WebsitePublishingLog
            {
                EpisodeId = episodeId,
                PublishedByUserId = session.UserId,
                PublishedAt = DateTime.UtcNow,
                MediaType = MediaType.Audio
            });
            episode.StatusId = EpisodeStatusValues.WebsitePublished;
        }
        else
        {
            var log = await context.WebsitePublishingLogs
                .Where(l => l.EpisodeId == episodeId && l.IsActive)
                .OrderByDescending(l => l.PublishedAt)
                .FirstOrDefaultAsync(cancellationToken);
            if (log != null) log.IsActive = false;
            episode.StatusId = EpisodeStatusValues.Published;
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// حذف الحلقة Async.
    /// </summary>
    public async Task<Result> DeleteEpisodeAsync(int episodeId, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodeDelete);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var episode = await context.Episodes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.EpisodeId == episodeId, cancellationToken);

            if (episode == null) return Result.Fail("الحلقة غير موجودة.");
            if (!episode.IsActive) return Result.Success();

            var guestChildren = await context.EpisodeGuests
                .Where(g => g.EpisodeId == episodeId && g.IsActive)
                .ToListAsync(cancellationToken);
            foreach (var g in guestChildren) g.IsActive = false;

            var corrChildren = await context.EpisodeCorrespondents
                .Where(c => c.EpisodeId == episodeId && c.IsActive)
                .ToListAsync(cancellationToken);
            foreach (var c in corrChildren) c.IsActive = false;

            var empChildren = await context.EpisodeEmployees
                .Where(e => e.EpisodeId == episodeId && e.IsActive)
                .ToListAsync(cancellationToken);
            foreach (var ee in empChildren) ee.IsActive = false;

            episode.IsActive = false;
            episode.UpdatedAt = DateTime.UtcNow;
            episode.UpdatedByUserId = session.UserId;

            await context.SaveChangesAsync(cancellationToken);

            _telemetryClient.TrackEvent("EpisodeDeleted", new Dictionary<string, string>
            {
                { "EpisodeId", episodeId.ToString() },
                { "UserId", session.UserId.ToString() }
            });

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during processing");
            _telemetryClient.TrackException(ex);
            return Result.Fail($"خطأ أثناء الحذف: {ex.Message}");
        }
    }

    public async Task<(int success, int fail)> CancelEpisodesBatchAsync(List<int> episodeIds, string reason, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodeManage);
        if (!permCheck.IsSuccess) return (0, episodeIds.Count);

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var affected = await context.Episodes
                .Where(e => episodeIds.Contains(e.EpisodeId) && e.IsActive && e.StatusId != EpisodeStatusValues.Cancelled)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.StatusId, EpisodeStatusValues.Cancelled)
                    .SetProperty(e => e.CancellationReason, reason)
                    .SetProperty(e => e.UpdatedAt, DateTime.UtcNow), cancellationToken);

            return (affected, episodeIds.Count - affected);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to batch cancel {Count} episodes", episodeIds.Count);
            return (0, episodeIds.Count);
        }
    }

    public async Task<(int success, int fail)> DeleteEpisodesBatchAsync(List<int> episodeIds, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodeDelete);
        if (!permCheck.IsSuccess) return (0, episodeIds.Count);

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            await context.EpisodeGuests
                .Where(eg => episodeIds.Contains(eg.EpisodeId) && eg.IsActive)
                .ExecuteUpdateAsync(s => s.SetProperty(eg => eg.IsActive, false), cancellationToken);

            await context.EpisodeCorrespondents
                .Where(ec => episodeIds.Contains(ec.EpisodeId) && ec.IsActive)
                .ExecuteUpdateAsync(s => s.SetProperty(ec => ec.IsActive, false), cancellationToken);

            await context.EpisodeEmployees
                .Where(ee => episodeIds.Contains(ee.EpisodeId) && ee.IsActive)
                .ExecuteUpdateAsync(s => s.SetProperty(ee => ee.IsActive, false), cancellationToken);

            var affected = await context.Episodes
                .Where(e => episodeIds.Contains(e.EpisodeId) && e.IsActive)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.IsActive, false)
                    .SetProperty(e => e.UpdatedAt, DateTime.UtcNow)
                    .SetProperty(e => e.UpdatedByUserId, session.UserId), cancellationToken);

            _telemetryClient.TrackEvent("EpisodesBatchDeleted", new Dictionary<string, string>
            {
                { "Count", affected.ToString() },
                { "UserId", session.UserId.ToString() }
            });

            return (affected, episodeIds.Count - affected);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to batch delete {Count} episodes", episodeIds.Count);
            return (0, episodeIds.Count);
        }
    }

    /// <summary>
    /// Is Valid Transition.
    /// </summary>
    private static bool IsValidTransition(byte fromStatus, byte toStatus)
    {
        return EpisodeStatusTransition.IsValid(fromStatus, toStatus);
    }

    private static void AddStatusAuditLog(
        BroadcastWorkflowDBContext context,
        int episodeId,
        byte oldStatusId,
        byte newStatusId,
        int? userId,
        string? reason)
    {
        var oldName = EpisodeStatusValues.GetDisplayName(oldStatusId);
        var newName = EpisodeStatusValues.GetDisplayName(newStatusId);

        var oldValues = JsonSerializer.Serialize(new { StatusId = oldStatusId, StatusName = oldName });
        var newValues = JsonSerializer.Serialize(new { StatusId = newStatusId, StatusName = newName });

        context.Set<AuditLog>().Add(new AuditLog
        {
            TableName = "Episodes",
            RecordId = episodeId,
            Action = "STATUS_CHANGE",
            OldValues = oldValues,
            NewValues = newValues,
            Reason = reason,
            UserId = userId,
            ChangedAt = DateTime.UtcNow
        });
    }
}
