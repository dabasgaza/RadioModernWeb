// ============================================================
// SocialMediaPlatformDto — منصة التواصل
// ============================================================
// المسؤولية: تعريف منصة التواصل.
// ============================================================
namespace DataAccess.DTOs;

/// <summary>
/// سجل منصة التواصل.
/// </summary>
public record SocialMediaPlatformDto(
    int SocialMediaPlatformId,
    string Name,
    string? Icon,
    string? BaseUrl = null);
