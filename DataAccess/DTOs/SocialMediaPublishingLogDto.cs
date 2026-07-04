// ============================================================
// SocialMediaPublishingLogDto — سجل النشر الرقمي
// ============================================================
// المسؤولية: تعريف سجل النشر الرقمي.
// ============================================================
using Domain.Models;

namespace DataAccess.DTOs;

/// <summary>
/// سجل سجل النشر الرقمي.
/// </summary>
public record SocialMediaPublishingLogDto(
    int LogId,
    int EpisodeGuestId,
    int EpisodeId,
    string? ClipTitle,
    TimeSpan? Duration,
    MediaType MediaType,
    List<PlatformPublishDto> Platforms);
