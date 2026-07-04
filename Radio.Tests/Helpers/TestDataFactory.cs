// ============================================================
// TestDataFactory — بيانات الاختبار
// ============================================================
// المسؤولية: تعريف بيانات الاختبار.
// ============================================================
using DataAccess.DTOs;
using Domain.Models;
using Radio.Web.ViewModels;

namespace Radio.Tests.Helpers;

/// <summary>
/// صنف بيانات الاختبار.
/// </summary>
public static class TestDataFactory
{
    public static SocialMediaPublishingLogDto CreateSocialLog(
        int episodeGuestId = 1, int episodeId = 1, string title = "Clip",
        MediaType mediaType = MediaType.Audio, params int[] platformIds)
    {
        var platforms = platformIds.Length > 0
            ? platformIds.Select(id => new PlatformPublishDto(id, "", "https://example.com")).ToList()
            : [new PlatformPublishDto(1, "", "https://facebook.com")];

        return new SocialMediaPublishingLogDto(
            LogId: 0, EpisodeGuestId: episodeGuestId, EpisodeId: episodeId,
            ClipTitle: title, Duration: TimeSpan.FromMinutes(5),
            MediaType: mediaType, Platforms: platforms);
    }

    public static GuestSocialLogFormItem CreateGuestLogFormItem(
        int episodeGuestId = 1, int episodeId = 1, string title = "Clip",
        MediaType mediaType = MediaType.Audio, int minutes = 5, int logId = 0)
        => new()
        {
            LogId = logId,
            EpisodeGuestId = episodeGuestId,
            EpisodeId = episodeId,
            ClipTitle = title,
            DurationMinutes = minutes,
            MediaType = mediaType,
            Platforms = [new PlatformUrlFormItem { PlatformId = 1, Url = "https://facebook.com" }]
        };

    /// <summary>
    /// إنشاء Social Form.
    /// </summary>
    public static SocialPublishingFormModel CreateSocialForm(params GuestSocialLogFormItem[] items)
        => new() { GuestLogs = [.. items] };
}
