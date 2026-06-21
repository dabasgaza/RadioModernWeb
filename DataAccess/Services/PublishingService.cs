using DataAccess.Common;
using DataAccess.DTOs;
using Domain.Models;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace DataAccess.Services;

public interface IPublishingQueryService
{
    Task<List<SocialMediaPlatformDto>> GetAllPlatformsAsync(CancellationToken cancellationToken = default);
    Task<SocialMediaPublishingLogDto?> GetSocialPublishingLogAsync(int episodeGuestId, CancellationToken cancellationToken = default);
    Task<SocialMediaPublishingLogDto?> GetSocialPublishingLogByIdAsync(int logId, CancellationToken cancellationToken = default);
    Task<List<SocialMediaPublishingLogDto>> GetEpisodeSocialLogsAsync(int episodeId, CancellationToken cancellationToken = default);
    Task<WebsitePublishingLogDto?> GetWebsitePublishingLogAsync(int episodeId, CancellationToken cancellationToken = default);
    Task<WebsitePublishingLogDto?> GetWebsitePublishingLogByIdAsync(int logId, CancellationToken cancellationToken = default);
    Task<List<PublishingRecordDto>> GetAllPublishingRecordsAsync(int? episodeId = null, CancellationToken cancellationToken = default);
}

public interface IPublishingCommandService
{
    Task<Result> LogWebsitePublishingAsync(int episodeId, string title, MediaType mediaType, string notes, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> LogSocialPublishingAsync(int episodeId, List<SocialMediaPublishingLogDto> guestLogs, UserSession session, CancellationToken cancellationToken = default);
    Task<Result<int>> SavePublishingLogAsync(SocialMediaPublishingLogDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result<int>> PublishToWebsiteAsync(WebsitePublishingLogDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> UpdateSocialPublishingLogAsync(SocialMediaPublishingLogDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> UpdateWebsitePublishingLogAsync(WebsitePublishingLogDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> DeleteSocialPublishingLogAsync(int logId, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> DeleteWebsitePublishingLogAsync(int logId, UserSession session, CancellationToken cancellationToken = default);
}

public interface IPublishingService : IPublishingQueryService, IPublishingCommandService { }

// ✨ استخدام Primary Constructor
public class PublishingService : IPublishingService
{
    private readonly IDbContextFactory<BroadcastWorkflowDBContext> _contextFactory;
    private readonly HybridCache _cache;
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<PublishingService> _logger;

    public PublishingService(
        IDbContextFactory<BroadcastWorkflowDBContext> contextFactory,
        HybridCache cache,
        TelemetryClient telemetryClient,
        ILogger<PublishingService> logger)
    {
        _contextFactory = contextFactory;
        _cache = cache;
        _telemetryClient = telemetryClient;
        _logger = logger;
    }
    public async Task<Result> LogWebsitePublishingAsync(int episodeId, string title, MediaType mediaType, string notes, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodePublish);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        using var context = await _contextFactory.CreateDbContextAsync();

        if (!await context.EnsureDomainUserExistsAsync(session))
            return Result.Fail("المستخدم غير موجود في النظام. الرجاء تسجيل الخروج وإعادة تسجيل الدخول.");

        var log = new WebsitePublishingLog
        {
            EpisodeId = episodeId,
            PublishedByUserId = session.UserId,
            Title = title,
            MediaType = mediaType,
            Notes = notes,
            PublishedAt = DateTime.UtcNow
        };

        context.WebsitePublishingLogs.Add(log);

        var episode = await context.Episodes.FindAsync(episodeId);

        if (episode == null)
            return Result.Fail("عذراً، لم يتم العثور على الحلقة المطلوبة.");

        if (episode.StatusId is not EpisodeStatusValues.Executed and not EpisodeStatusValues.Published)
            return Result.Fail("لا يمكن نشر حلقة لم يتم توثيق تنفيذها (الإنتاج) أولاً.");

        episode.StatusId = EpisodeStatusValues.WebsitePublished;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> LogSocialPublishingAsync(int episodeId, List<SocialMediaPublishingLogDto> guestLogs, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodePublish);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        using var context = await _contextFactory.CreateDbContextAsync();

        if (!await context.EnsureDomainUserExistsAsync(session))
            return Result.Fail("المستخدم غير موجود في النظام. الرجاء تسجيل الخروج وإعادة تسجيل الدخول.");

        // 1. التحقق من وجود الحلقة
        var episode = await context.Episodes.FindAsync(episodeId);
        if (episode == null)
            return Result.Fail("عذراً، لم يتم العثور على الحلقة المطلوبة.");

        if (episode.StatusId is not EpisodeStatusValues.Executed and not EpisodeStatusValues.WebsitePublished)
            return Result.Fail("لا يمكن نشر حلقة لم يتم توثيق تنفيذها (الإنتاج) أولاً.");

        var now = DateTime.UtcNow;

        // 2. الحصول على جميع كائنات ضيوف الحلقة المرتبطة بسجلات النشر دفعة واحدة لتفادي استعلام N+1
        var guestIds = guestLogs.Select(g => g.EpisodeGuestId).ToList();
        var episodeGuests = await context.EpisodeGuests
            .Where(eg => guestIds.Contains(eg.EpisodeGuestId))
            .ToListAsync(cancellationToken);

        // 3. إنشاء سجلات النشر لكل ضيف وإضافة منصات النشر المرافقة
        foreach (var g in guestLogs)
        {
            var log = new SocialMediaPublishingLog
            {
                EpisodeGuestId = g.EpisodeGuestId,
                PublishedByUserId = session.UserId,
                MediaType = g.MediaType,
                ClipTitle = g.ClipTitle,
                ClipDuration = g.Duration,
                PublishedAt = now
            };

            foreach (var p in g.Platforms)
            {
                log.Platforms.Add(new SocialMediaPublishingLogPlatform
                {
                    SocialMediaPlatformId = p.PlatformId,
                    Url = p.Url
                });
            }

            context.SocialMediaPublishingLogs.Add(log);

            // 4. تحديث ClipStatus للضيف من الذاكرة
            var episodeGuest = episodeGuests.FirstOrDefault(eg => eg.EpisodeGuestId == g.EpisodeGuestId);
            if (episodeGuest != null)
            {
                episodeGuest.ClipStatus = GuestClipStatus.Published;
            }
        }

        // 5. تحديث حالة الحلقة
        episode.StatusId = EpisodeStatusValues.Published;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// الحصول على جميع منصات السوشيال ميديا المتاحة
    /// </summary>
    public async Task<List<SocialMediaPlatformDto>> GetAllPlatformsAsync(CancellationToken cancellationToken = default)
    {
        var operation = _telemetryClient.StartOperation<Microsoft.ApplicationInsights.DataContracts.RequestTelemetry>("GetAllPlatforms");
        try
        {
            var result = await _cache.GetOrCreateAsync("platforms", async _ =>
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SocialMediaPlatforms
                    .AsNoTracking()
                    .Where(p => p.IsActive)
                    .Select(p => new SocialMediaPlatformDto(p.SocialMediaPlatformId, p.Name, p.Icon))
                    .ToListAsync(cancellationToken);
            }, new HybridCacheEntryOptions { Expiration = TimeSpan.FromHours(1) }) ?? [];
            _telemetryClient.TrackMetric("PlatformsCount", result.Count);
            operation.Telemetry.Success = true;
            return result;
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
    /// حفظ سجل نشر اجتماعي لضيف مع المنصات والروابط
    /// </summary>
    public async Task<Result<int>> SavePublishingLogAsync(SocialMediaPublishingLogDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodePublish);
        if (!permCheck.IsSuccess) return Result<int>.Fail(permCheck.ErrorMessage!);

        using var context = await _contextFactory.CreateDbContextAsync();

        if (!await context.EnsureDomainUserExistsAsync(session))
            return Result<int>.Fail("المستخدم غير موجود في النظام. الرجاء تسجيل الخروج وإعادة تسجيل الدخول.");

        try
        {
            var log = new SocialMediaPublishingLog
            {
                EpisodeGuestId = dto.EpisodeGuestId,
                MediaType = dto.MediaType,
                ClipTitle = dto.ClipTitle,
                ClipDuration = dto.Duration,
                PublishedAt = DateTime.UtcNow,
                PublishedByUserId = session.UserId
            };

            // إضافة المنصات والروابط عبر navigation property لتجنب الحاجة إلى SaveChangesAsync منفصل
            foreach (var platform in dto.Platforms)
            {
                log.Platforms.Add(new SocialMediaPublishingLogPlatform
                {
                    SocialMediaPlatformId = platform.PlatformId,
                    Url = platform.Url
                });
            }

            context.SocialMediaPublishingLogs.Add(log);
            await context.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(log.SocialMediaPublishingLogId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during processing");
            return Result<int>.Fail($"خطأ في حفظ سجل النشر: {ex.Message}");
        }
    }

    /// <summary>
    /// نشر حلقة على الموقع الإلكتروني
    /// </summary>
    public async Task<Result<int>> PublishToWebsiteAsync(WebsitePublishingLogDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodeWebPublish);
        if (!permCheck.IsSuccess) return Result<int>.Fail(permCheck.ErrorMessage!);

        using var context = await _contextFactory.CreateDbContextAsync();

        if (!await context.EnsureDomainUserExistsAsync(session))
            return Result<int>.Fail("المستخدم غير موجود في النظام. الرجاء تسجيل الخروج وإعادة تسجيل الدخول.");

        try
        {
            // ✅ التحقق من وجود الحلقة أولاً قبل الوصول إلى خصائصها
            var episode = await context.Episodes.FindAsync(dto.EpisodeId);
            if (episode == null) return Result<int>.Fail("الحلقة غير موجودة");

            // التحقق من أن حالة الحلقة تسمح بنشر الموقع
            if (episode.StatusId < EpisodeStatusValues.Executed)
                return Result<int>.Fail("يجب تنفيذ الحلقة أولاً قبل نشرها على الموقع.");

            var log = new WebsitePublishingLog
            {
                EpisodeId = dto.EpisodeId,
                Title = dto.Title,
                MediaType = Enum.TryParse<MediaType>(dto.MediaType, true, out var parsedMediaType) ? parsedMediaType : MediaType.Audio,
                Notes = dto.Notes,
                PublishedAt = DateTime.UtcNow,
                PublishedByUserId = session.UserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.WebsitePublishingLogs.Add(log);

            // تحديث حالة الحلقة إلى WebsitePublished
            episode.StatusId = EpisodeStatusValues.WebsitePublished;

            await context.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(log.WebsitePublishingLogId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during processing");
            return Result<int>.Fail($"خطأ في نشر الحلقة: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════════
    //  استرجاع وتعديل سجلات النشر
    // ═══════════════════════════════════════════

    /// <summary>
    /// استرجاع سجل النشر الرقمي لضيف معيّن
    /// يُرجع null إذا لم يوجد سجل نشط لهذا الضيف
    /// </summary>
    public async Task<SocialMediaPublishingLogDto?> GetSocialPublishingLogAsync(int episodeGuestId, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // ✅ Select مباشر بدلاً من Include + تحويل في الذاكرة
        var log = await context.SocialMediaPublishingLogs
            .AsNoTracking()
            .Where(l => l.EpisodeGuestId == episodeGuestId && l.IsActive)
            .OrderByDescending(l => l.PublishedAt)
            .Select(l => new
            {
                l.SocialMediaPublishingLogId,
                l.EpisodeGuestId,
                EpisodeId = l.EpisodeGuest.EpisodeId,
                l.ClipTitle,
                l.ClipDuration,
                l.MediaType,
                Platforms = l.Platforms
                    .Where(p => p.IsActive)
                    .Select(p => new PlatformPublishDto(
                        p.SocialMediaPlatformId,
                        p.SocialMediaPlatform.Name,
                        p.Url))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (log is null) return null;

        return new SocialMediaPublishingLogDto(
            log.SocialMediaPublishingLogId,
            log.EpisodeGuestId,
            log.EpisodeId,
            log.ClipTitle,
            log.ClipDuration,
            log.MediaType,
            log.Platforms);
    }

    public async Task<SocialMediaPublishingLogDto?> GetSocialPublishingLogByIdAsync(int logId, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var log = await context.SocialMediaPublishingLogs
            .AsNoTracking()
            .Where(l => l.SocialMediaPublishingLogId == logId && l.IsActive)
            .Select(l => new
            {
                l.SocialMediaPublishingLogId,
                l.EpisodeGuestId,
                EpisodeId = l.EpisodeGuest.EpisodeId,
                l.ClipTitle,
                l.ClipDuration,
                l.MediaType,
                Platforms = l.Platforms
                    .Where(p => p.IsActive)
                    .Select(p => new PlatformPublishDto(
                        p.SocialMediaPlatformId,
                        p.SocialMediaPlatform.Name,
                        p.Url))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (log is null) return null;

        return new SocialMediaPublishingLogDto(
            log.SocialMediaPublishingLogId,
            log.EpisodeGuestId,
            log.EpisodeId,
            log.ClipTitle,
            log.ClipDuration,
            log.MediaType,
            log.Platforms);
    }

    /// <summary>
    /// استرجاع جميع سجلات النشر الرقمي لحلقة معيّنة
    /// كل ضيف في الحلقة قد يكون له سجل واحد
    /// </summary>
    public async Task<List<SocialMediaPublishingLogDto>> GetEpisodeSocialLogsAsync(int episodeId, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // ✅ Select مباشر بدلاً من Include ثلاثي + تحويل في الذاكرة
        return await context.SocialMediaPublishingLogs
            .AsNoTracking()
            .Where(l => l.EpisodeGuest.EpisodeId == episodeId && l.IsActive)
            .OrderByDescending(l => l.PublishedAt)
            .Select(l => new SocialMediaPublishingLogDto(
                l.SocialMediaPublishingLogId,
                l.EpisodeGuestId,
                l.EpisodeGuest.EpisodeId,
                l.ClipTitle,
                l.ClipDuration,
                l.MediaType,
                l.Platforms
                    .Where(p => p.IsActive)
                    .Select(p => new PlatformPublishDto(
                        p.SocialMediaPlatformId,
                        p.SocialMediaPlatform.Name,
                        p.Url))
                    .ToList()))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// تعديل سجل نشر رقمي موجود
    /// يحدّث: عنوان المقطع، المدة، نوع الوسائط
    /// ويستبدل جميع روابط المنصات (يحذف القديمة ويضيف الجديدة)
    /// </summary>
    public async Task<Result> UpdateSocialPublishingLogAsync(SocialMediaPublishingLogDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodePublish);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        using var context = await _contextFactory.CreateDbContextAsync();

        try
        {
            // 1. البحث عن السجل الموجود
            var log = await context.SocialMediaPublishingLogs
                .Include(l => l.Platforms)
                .FirstOrDefaultAsync(l => l.SocialMediaPublishingLogId == dto.LogId && l.IsActive, cancellationToken);

            if (log is null)
                return Result.Fail("سجل النشر غير موجود أو تم حذفه.");

            // 2. تحديث الحقول الأساسية
            log.ClipTitle = dto.ClipTitle;
            log.ClipDuration = dto.Duration;
            log.MediaType = dto.MediaType;
            log.UpdatedByUserId = session.UserId;

            // 3. حذف روابط المنصات القديمة (soft-delete)
            foreach (var oldPlatform in log.Platforms)
            {
                oldPlatform.IsActive = false;
            }

            // 4. إضافة روابط المنصات الجديدة
            foreach (var platform in dto.Platforms)
            {
                var newLink = new SocialMediaPublishingLogPlatform
                {
                    SocialMediaPublishingLogId = log.SocialMediaPublishingLogId,
                    SocialMediaPlatformId = platform.PlatformId,
                    Url = platform.Url
                };
                context.SocialMediaPublishingLogPlatforms.Add(newLink);
            }

            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// استرجاع سجل نشر الموقع الإلكتروني لحلقة معيّنة
    /// يُرجع null إذا لم يوجد سجل نشط
    /// </summary>
    public async Task<WebsitePublishingLogDto?> GetWebsitePublishingLogAsync(int episodeId, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var log = await context.WebsitePublishingLogs
            .AsNoTracking()
            .Where(l => l.EpisodeId == episodeId && l.IsActive)
            .OrderByDescending(l => l.PublishedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (log is null) return null;

        return new WebsitePublishingLogDto(
            log.WebsitePublishingLogId,
            log.EpisodeId,
            log.MediaType.ToString(),
            log.Title,
            log.Notes,
            log.PublishedAt);
    }

    public async Task<WebsitePublishingLogDto?> GetWebsitePublishingLogByIdAsync(int logId, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var log = await context.WebsitePublishingLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.WebsitePublishingLogId == logId && l.IsActive, cancellationToken);

        if (log is null) return null;

        return new WebsitePublishingLogDto(
            log.WebsitePublishingLogId,
            log.EpisodeId,
            log.MediaType.ToString(),
            log.Title,
            log.Notes,
            log.PublishedAt);
    }

    /// <summary>
    /// تعديل سجل نشر الموقع الإلكتروني
    /// يحدّث: العنوان، نوع الوسائط، الملاحظات
    /// </summary>
    public async Task<Result> UpdateWebsitePublishingLogAsync(WebsitePublishingLogDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodeWebPublish);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        using var context = await _contextFactory.CreateDbContextAsync();

        var log = await context.WebsitePublishingLogs
            .FirstOrDefaultAsync(l => l.WebsitePublishingLogId == dto.Id && l.IsActive, cancellationToken);

        if (log is null)
            return Result.Fail("سجل نشر الموقع غير موجود أو تم حذفه.");

        // تحديث الحقول القابلة للتعديل
        log.Title = dto.Title;
        log.MediaType = Enum.TryParse<MediaType>(dto.MediaType, true, out var mt) ? mt : MediaType.Audio;
        log.Notes = dto.Notes;
        log.UpdatedByUserId = session.UserId;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteSocialPublishingLogAsync(int logId, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodePublish);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        using var context = await _contextFactory.CreateDbContextAsync();

        var log = await context.SocialMediaPublishingLogs
            .Include(l => l.Platforms)
            .FirstOrDefaultAsync(l => l.SocialMediaPublishingLogId == logId && l.IsActive, cancellationToken);

        if (log is null)
            return Result.Fail("سجل النشر غير موجود أو تم حذفه مسبقاً.");

        log.IsActive = false;
        log.UpdatedByUserId = session.UserId;

        foreach (var p in log.Platforms)
            p.IsActive = false;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteWebsitePublishingLogAsync(int logId, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.EpisodeWebPublish);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        using var context = await _contextFactory.CreateDbContextAsync();

        var log = await context.WebsitePublishingLogs
            .FirstOrDefaultAsync(l => l.WebsitePublishingLogId == logId && l.IsActive, cancellationToken);

        if (log is null)
            return Result.Fail("سجل نشر الموقع غير موجود أو تم حذفه مسبقاً.");

        log.IsActive = false;
        log.UpdatedByUserId = session.UserId;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// استرجاع قائمة موحّدة من جميع سجلات النشر (الأنواع الثلاثة)
    /// تُستخدم في شاشة العرض الشامل مع دعم الفلترة حسب الحلقة
    /// </summary>
    public async Task<List<PublishingRecordDto>> GetAllPublishingRecordsAsync(int? episodeId = null, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var records = new List<PublishingRecordDto>();

        // 1. سجلات التنفيذ — AsSplitQuery + Select مباشر
        var execQuery = context.ExecutionLogs
            .AsNoTracking()
            .AsSplitQuery()
            .Where(l => l.IsActive);

        if (episodeId.HasValue)
            execQuery = execQuery.Where(l => l.EpisodeId == episodeId.Value);

        var execLogs = await execQuery
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new PublishingRecordDto
            {
                RecordId = l.ExecutionLogId,
                RecordType = "Execution",
                EpisodeId = l.EpisodeId,
                EpisodeName = l.Episode != null ? l.Episode.EpisodeName : null,
                ProgramName = l.Episode != null && l.Episode.Program != null ? l.Episode.Program.ProgramName : null,
                ProgramId = l.Episode != null ? (int?)l.Episode.ProgramId : null,
                Summary = l.DurationMinutes.HasValue ? $"مدة التنفيذ: {l.DurationMinutes} دقيقة" : null,
                RecordDate = l.CreatedAt,
                RecordedBy = l.ExecutedByUser != null ? l.ExecutedByUser.FullName : null,
                RecordIcon = "PlayCircleOutline",
                RecordColor = "#4CAF50"
            })
            .ToListAsync(cancellationToken);

        records.AddRange(execLogs);

        // 2. سجلات النشر الرقمي (سوشال ميديا) - جلب البيانات الخام أولاً
        var socialQuery = context.SocialMediaPublishingLogs
            .AsNoTracking()
            .Where(l => l.IsActive);

        if (episodeId.HasValue)
            socialQuery = socialQuery.Where(l => l.EpisodeGuest.EpisodeId == episodeId.Value);

        var rawSocialLogs = await socialQuery
            .Select(l => new
            {
                l.SocialMediaPublishingLogId,
                EpisodeId = l.EpisodeGuest.EpisodeId,
                EpisodeName = l.EpisodeGuest.Episode != null ? l.EpisodeGuest.Episode.EpisodeName : null,
                ProgramName = l.EpisodeGuest.Episode != null && l.EpisodeGuest.Episode.Program != null ? l.EpisodeGuest.Episode.Program.ProgramName : null,
                ProgramId = l.EpisodeGuest.Episode != null ? (int?)l.EpisodeGuest.Episode.ProgramId : null,
                l.PublishedAt,
                RecordedBy = l.PublishedByUser != null ? l.PublishedByUser.FullName : null,
                GuestName = l.EpisodeGuest.Guest != null ? l.EpisodeGuest.Guest.FullName : "ضيف غير معروف",
                Platforms = l.Platforms
                    .Where(p => p.IsActive)
                    .Select(p => p.SocialMediaPlatform != null ? p.SocialMediaPlatform.Name : "منصة غير معروف")
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        // 2. سجلات النشر الرقمي (سوشال ميديا) - تجميعها حسب الحلقة لتعرض في سطر واحد لكل حلقة
        var socialLogs = rawSocialLogs
            .GroupBy(l => l.EpisodeId)
            .Select(g =>
            {
                var first = g.First();
                var guestsList = string.Join("، ", g.Select(x => x.GuestName).Distinct());
                var allPlatforms = g.SelectMany(x => x.Platforms).Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList();
                var platformsStr = allPlatforms.Count > 0 ? $" ({string.Join("، ", allPlatforms)})" : "";

                return new PublishingRecordDto
                {
                    RecordId = first.SocialMediaPublishingLogId, // استخدام معرف السجل الأول للتمكن من تعديله
                    RecordType = "SocialMedia",
                    EpisodeId = g.Key,
                    EpisodeName = first.EpisodeName,
                    ProgramName = first.ProgramName,
                    ProgramId = first.ProgramId,
                    Summary = $"نشر مقاطع للضيوف: {guestsList}{platformsStr}",
                    RecordDate = g.Max(x => x.PublishedAt), // تاريخ أحدث عملية نشر
                    RecordedBy = string.Join("، ", g.Select(x => x.RecordedBy).Where(x => !string.IsNullOrEmpty(x)).Distinct()),
                    RecordIcon = "ShareVariant",
                    RecordColor = "#2196F3"
                };
            })
            .ToList();

        records.AddRange(socialLogs);

        // 3. سجلات نشر الموقع الإلكتروني — Select مباشر + AsSplitQuery
        var webQuery = context.WebsitePublishingLogs
            .AsNoTracking()
            .AsSplitQuery()
            .Where(l => l.IsActive);

        if (episodeId.HasValue)
            webQuery = webQuery.Where(l => l.EpisodeId == episodeId.Value);

        var webLogs = await webQuery
            .OrderByDescending(l => l.PublishedAt)
            .Select(l => new PublishingRecordDto
            {
                RecordId = l.WebsitePublishingLogId,
                RecordType = "Website",
                EpisodeId = l.EpisodeId,
                EpisodeName = l.Episode != null ? l.Episode.EpisodeName : null,
                ProgramName = l.Episode != null && l.Episode.Program != null ? l.Episode.Program.ProgramName : null,
                ProgramId = l.Episode != null ? (int?)l.Episode.ProgramId : null,
                Summary = l.Title ?? "بدون عنوان",
                RecordDate = l.PublishedAt,
                RecordedBy = l.PublishedByUser != null ? l.PublishedByUser.FullName : null,
                RecordIcon = "Web",
                RecordColor = "#5C6BC0"
            })
            .ToListAsync(cancellationToken);

        records.AddRange(webLogs);

        // ترتيب نهائي: الأحدث أولاً
        return records
            .OrderByDescending(r => r.RecordDate)
            .ToList();
    }
}
