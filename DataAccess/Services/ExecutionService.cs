// ============================================================
// ExecutionService — ExecutionService
// ============================================================
// المسؤولية: تعريف ExecutionService.
// ============================================================
using DataAccess.Common;
using DataAccess.DTOs;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Services;

/// <summary>
/// واجهة I Execution.
/// </summary>
public interface IExecutionService
{
    // ✨ استقبال DTO بدلاً من الكيان
    Task<Result> LogExecutionAsync(ExecutionLogDto dto, UserSession session, CancellationToken cancellationToken = default);

    Task<ExecutionLogDto?> GetExecutionLogAsync(int episodeId, CancellationToken cancellationToken = default);

    Task<Result> UpdateExecutionLogAsync(ExecutionLogDto dto, UserSession session, CancellationToken cancellationToken = default);
}

// ✨ استخدام Primary Constructor
/// <summary>
/// صنف ExecutionService.
/// </summary>
public class ExecutionService(IDbContextFactory<BroadcastWorkflowDBContext> contextFactory) : IExecutionService
{
    /// <summary>
    /// تسجيل Execution Async.
    /// </summary>
    public async Task<Result> LogExecutionAsync(ExecutionLogDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodeExecute);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        using var context = await contextFactory.CreateDbContextAsync();

        try
        {
            var episode = await context.Episodes.FindAsync(dto.EpisodeId);
            if (episode == null) return Result.Fail("الحلقة غير موجودة.");

            if (!await context.EnsureDomainUserExistsAsync(session))
                return Result.Fail("المستخدم الحالي غير موجود في قاعدة البيانات. الرجاء تسجيل الخروج ثم تسجيل الدخول مجدداً.");

            var log = new ExecutionLog
            {
                EpisodeId = dto.EpisodeId,
                ExecutedByUserId = session.UserId,
                DurationMinutes = dto.DurationMinutes,
                ExecutionNotes = dto.ExecutionNotes,
                IssuesEncountered = dto.IssuesEncountered
            };

            context.ExecutionLogs.Add(log);

            // ✨ استخدام الثوابت بدلاً من الأرقام السحرية
            if (episode.StatusId == EpisodeStatusValues.Published)
                return Result.Fail("لا يمكن تعديل حالة حلقة تم نشرها بالفعل.");

            episode.StatusId = EpisodeStatusValues.Executed;
            episode.ActualExecutionTime = DateTime.UtcNow;

            // ❌ تم إزالة UpdatedAt و UpdatedByUserId (الـ Interceptor سيتولى الأمر تلقائياً)

            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch
        {
            // ✨ رمي الاستثناء الأصلي كما هو للحفاظ على الـ Stack Trace (بدون تغليف)
            throw;
        }
    }

    // ═══════════════════════════════════════════
    //  استرجاع وتعديل سجل التنفيذ
    // ═══════════════════════════════════════════

    /// <summary>
    /// استرجاع سجل التنفيذ لحلقة معيّنة
    /// يُرجع null إذا لم يوجد سجل تنفيذ نشط
    /// <summary>
    /// استرجاع Execution سجل Async.
    /// </summary>
    /// </summary>
    public async Task<ExecutionLogDto?> GetExecutionLogAsync(int episodeId, CancellationToken cancellationToken = default)
    {
        // لا نحتاج صلاحية خاصة — مجرد قراءة
        using var context = await contextFactory.CreateDbContextAsync();

        var log = await context.ExecutionLogs
            .AsNoTracking()
            .Where(l => l.EpisodeId == episodeId && l.IsActive)
            .OrderByDescending(l => l.CreatedAt)  // أحدث سجل أولاً
            .FirstOrDefaultAsync(cancellationToken);

        if (log is null) return null;

        // تحويل الكيان إلى DTO
        return new ExecutionLogDto
        {
            ExecutionLogId = log.ExecutionLogId,
            EpisodeId = log.EpisodeId,
            ExecutedByUserId = log.ExecutedByUserId,
            ExecutionNotes = log.ExecutionNotes,
            IssuesEncountered = log.IssuesEncountered,
            DurationMinutes = log.DurationMinutes ?? 0
        };
    }

    /// <summary>
    /// تعديل سجل تنفيذ موجود
    /// يحدّث: المدة، الملاحظات، المشاكل التقنية
    /// لا يُغيّر حالة الحلقة — فقط يحدّث البيانات
    /// <summary>
    /// تحديث Execution سجل Async.
    /// </summary>
    /// </summary>
    public async Task<Result> UpdateExecutionLogAsync(ExecutionLogDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodeExecute);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        using var context = await contextFactory.CreateDbContextAsync();

        var log = await context.ExecutionLogs
            .FirstOrDefaultAsync(l => l.ExecutionLogId == dto.ExecutionLogId && l.IsActive, cancellationToken);

        if (log is null)
            return Result.Fail("سجل التنفيذ غير موجود أو تم حذفه.");

        // تحديث الحقول القابلة للتعديل فقط
        log.DurationMinutes = dto.DurationMinutes;
        log.ExecutionNotes = dto.ExecutionNotes;
        log.IssuesEncountered = dto.IssuesEncountered;
        log.UpdatedByUserId = session.UserId;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}