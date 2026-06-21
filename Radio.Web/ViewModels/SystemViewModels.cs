using DataAccess.Services;
using Domain.Models;

namespace Radio.Web.ViewModels;

public class SocialPublishingViewModel
{
    public DataAccess.DTOs.ActiveEpisodeDto? Episode { get; set; }
    public List<DataAccess.DTOs.EpisodeGuestDto> EpisodeGuests { get; set; } = new();
    public List<DataAccess.DTOs.SocialMediaPlatformDto> Platforms { get; set; } = new();
}

public class SocialPublishingFormModel
{
    public List<GuestSocialLogFormItem> GuestLogs { get; set; } = new();
}

public class GuestSocialLogFormItem
{
    public int LogId { get; set; }
    public int EpisodeGuestId { get; set; }
    public int EpisodeId { get; set; }
    public string ClipTitle { get; set; } = "";
    public int? DurationMinutes { get; set; }
    public MediaType MediaType { get; set; }
    public List<PlatformUrlFormItem> Platforms { get; set; } = new();
    public string? GuestName { get; set; }
}

public class PlatformUrlFormItem
{
    public int PlatformId { get; set; }
    public string? Url { get; set; }
}

public class SocialPublishingEditViewModel
{
    public DataAccess.DTOs.SocialMediaPublishingLogDto? Log { get; set; }
    public DataAccess.DTOs.ActiveEpisodeDto? Episode { get; set; }
    public List<DataAccess.DTOs.SocialMediaPlatformDto> Platforms { get; set; } = new();
    public List<GuestSocialLogFormItem> GuestLogs { get; set; } = new();
    public List<DataAccess.DTOs.PublishingRecordDto> EpisodePublishingRecords { get; set; } = new();
}

public class WebsitePublishEditViewModel
{
    public DataAccess.DTOs.WebsitePublishingLogDto? Log { get; set; }
    public DataAccess.DTOs.ActiveEpisodeDto? Episode { get; set; }
    public List<DataAccess.DTOs.PublishingRecordDto> EpisodePublishingRecords { get; set; } = new();
}

public class ReportsViewModel
{
    public List<DataAccess.DTOs.TodayEpisodeDto> TodayEpisodes { get; set; } = new();
    public Dictionary<string, int> StatusStats { get; set; } = new();
    public List<DataAccess.DTOs.ActiveProgramDto> TopPrograms { get; set; } = new();
    public List<DataAccess.DTOs.TopGuestDto> TopGuests { get; set; } = new();
    public List<DataAccess.DTOs.CancelledEpisodeDto> CancelledEpisodes { get; set; } = new();
}

public class DiagnosticsViewModel
{
    public DiagnosticsSummaryDto Summary { get; set; } = new();
    public List<DiagnosticLogDto> Logs { get; set; } = new();
}

public class ProgramViewModel
{
    public DataAccess.DTOs.ProgramDto Program { get; set; } = null!;
    public int EpisodeCount { get; set; }
}

public class DatabaseDashboardViewModel
{
    public long DatabaseSizeBytes { get; set; }
    public long DatabaseLogSizeBytes { get; set; }
    public DateTime? LastBackupAt { get; set; }
    public long LastBackupSizeBytes { get; set; }
    public int TotalBackups { get; set; }
    public double SuccessRate { get; set; }
    public int BackupsThisMonth { get; set; }
    public int ActiveConnections { get; set; }
    public bool IsAutoBackupEnabled { get; set; }
    public bool IsCloudSyncEnabled { get; set; }
    public int RetentionDays { get; set; }
    public long BackupFolderSizeBytes { get; set; }
    public int FailureCount { get; set; }
    public string DatabaseName { get; set; } = "";
    public long TotalSizeBytes => DatabaseSizeBytes + DatabaseLogSizeBytes;
    public string DatabaseSizeFormatted => FormatBytes(DatabaseSizeBytes);
    public string LogSizeFormatted => FormatBytes(DatabaseLogSizeBytes);
    public string TotalSizeFormatted => FormatBytes(TotalSizeBytes);
    public string LastBackupSizeFormatted => FormatBytes(LastBackupSizeBytes);
    public string BackupFolderSizeFormatted => FormatBytes(BackupFolderSizeBytes);
    public string SuccessRateFormatted => $"{SuccessRate:F0}%";
    public TimeSpan? TimeSinceLastBackup => LastBackupAt.HasValue ? DateTime.UtcNow - LastBackupAt.Value : null;
    public string TimeSinceLastBackupFormatted
    {
        get
        {
            if (TimeSinceLastBackup == null) return "لا يوجد";
            var ts = TimeSinceLastBackup.Value;
            if (ts.TotalHours < 1) return $"منذ {ts.TotalMinutes:F0} دقيقة";
            if (ts.TotalDays < 1) return $"منذ {ts.TotalHours:F0} ساعة";
            return $"منذ {ts.TotalDays:F0} يوم";
        }
    }
    public bool IsHealthy => SuccessRate >= 80 && (TimeSinceLastBackup == null || TimeSinceLastBackup.Value.TotalDays < 7) && FailureCount < 5;
    public string HealthStatus => IsHealthy ? "سليمة" : "تحتاج مراجعة";
    public string HealthColor => IsHealthy ? "success" : "danger";

    public List<DatabaseBackupLog> BackupLogs { get; set; } = new();

    static string FormatBytes(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024L * 1024 => $"{bytes / 1024.0:F1} KB",
        < 1024L * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
        _ => $"{bytes / (1024.0 * 1024 * 1024):F2} GB"
    };
}

