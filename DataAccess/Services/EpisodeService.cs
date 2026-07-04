// ============================================================
// EpisodeService — الحلقات
// ============================================================
// المسؤولية: تعريف الحلقات.
// ============================================================
using DataAccess.Common;
using DataAccess.DTOs;
using Domain.Models;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Services;

/// <summary>
/// واجهة I الحلقة استعلام.
/// </summary>
public interface IEpisodeQueryService
{
    Task<List<ActiveEpisodeDto>> GetActiveEpisodesAsync(CancellationToken cancellationToken = default);
    Task<ActiveEpisodeDto?> GetActiveEpisodeByIdAsync(int episodeId, CancellationToken cancellationToken = default);
    Task<List<EpisodeGuestDto>> GetEpisodeGuestsAsync(int episodeId, CancellationToken cancellationToken = default);
    Task<List<ConflictInfo>> GetConflictingEpisodesAsync(int programId, DateTime scheduledTime, int? excludeEpisodeId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// واجهة I الحلقة أمر.
/// </summary>
public interface IEpisodeCommandService
{
    Task<Result<int>> CreateEpisodeAsync(EpisodeDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> UpdateEpisodeAsync(EpisodeDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> UpdateStatusAsync(int episodeId, byte newStatusId, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> DeleteEpisodeAsync(int episodeId, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> ToggleWebsitePublishAsync(int episodeId, bool isPublished, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> RevertEpisodeStatusAsync(int episodeId, string reason, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> CancelEpisodeAsync(int episodeId, string reason, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> UpdateCancellationReasonAsync(int episodeId, string newReason, UserSession session, CancellationToken cancellationToken = default);
    Task<(int success, int fail)> CancelEpisodesBatchAsync(List<int> episodeIds, string reason, UserSession session, CancellationToken cancellationToken = default);
    Task<(int success, int fail)> DeleteEpisodesBatchAsync(List<int> episodeIds, UserSession session, CancellationToken cancellationToken = default);
}

/// <summary>
/// واجهة I الحلقة.
/// </summary>
public interface IEpisodeService : IEpisodeQueryService, IEpisodeCommandService { }

/// <summary>
/// صنف الحلقات.
/// </summary>
public partial class EpisodeService : IEpisodeService
{
    private readonly IDbContextFactory<BroadcastWorkflowDBContext> _contextFactory;
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<EpisodeService> _logger;

    public EpisodeService(
        IDbContextFactory<BroadcastWorkflowDBContext> contextFactory,
        TelemetryClient telemetryClient,
        ILogger<EpisodeService> logger)
    {
        _contextFactory = contextFactory;
        _telemetryClient = telemetryClient;
        _logger = logger;
    }
}

/// <summary>
/// صنف الحلقة الحالة Values.
/// </summary>
public static class EpisodeStatusValues
{
    public const byte Planned = 0;
    public const byte Executed = 1;
    public const byte Published = 2;
    public const byte WebsitePublished = 3;
    public const byte Cancelled = 4;

    /// <summary>
    /// استرجاع Display الاسم.
    /// </summary>
    public static string GetDisplayName(byte statusId) => statusId switch
    {
        Planned => "مجدولة",
        Executed => "تم التنفيذ",
        Published => "منشورة رقمياً",
        WebsitePublished => "منشورة على الموقع",
        Cancelled => "ملغاة",
        _ => $"غير معروفة ({statusId})"
    };
}

/// <summary>
/// سجل Conflict معلومات.
/// </summary>
public record ConflictInfo(int EpisodeId, string EpisodeName, string ProgramName, DateTime ScheduledTime, ConflictLevel Level);
public enum ConflictLevel { Medium, High }
