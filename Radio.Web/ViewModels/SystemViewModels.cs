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

