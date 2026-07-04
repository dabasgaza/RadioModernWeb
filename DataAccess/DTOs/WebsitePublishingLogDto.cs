// ============================================================
// WebsitePublishingLogDto — سجل نشر الموقع
// ============================================================
// المسؤولية: تعريف سجل نشر الموقع.
// ============================================================
namespace DataAccess.DTOs;

/// <summary>
/// سجل سجل نشر الموقع.
/// </summary>
public record WebsitePublishingLogDto(
    int Id,
    int EpisodeId,
    string? MediaType,
    string? Title,
    string? Notes,
    DateTime PublishedAt);