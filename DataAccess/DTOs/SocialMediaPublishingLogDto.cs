using Domain.Models;

namespace DataAccess.DTOs;

public record SocialMediaPublishingLogDto(
    int LogId,
    int EpisodeGuestId,
    int EpisodeId,
    string? ClipTitle,
    TimeSpan? Duration,
    MediaType MediaType,
    List<PlatformPublishDto> Platforms);
