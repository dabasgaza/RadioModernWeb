// ============================================================
// PlatformPublishDto — نشر المنصة
// ============================================================
// المسؤولية: تعريف نشر المنصة.
// ============================================================
namespace DataAccess.DTOs;

/// <summary>
/// سجل نشر المنصة.
/// </summary>
public record PlatformPublishDto(int PlatformId, string PlatformName, string? Url);