// ============================================================
// SocialMediaPlatform — منصة التواصل
// ============================================================
// المسؤولية: تعريف منصة التواصل.
// ============================================================
namespace Domain.Models;

/// <summary>
/// صنف منصة التواصل.
/// </summary>
public class SocialMediaPlatform : BaseEntity
{
    public int SocialMediaPlatformId { get; set; }

    public string Name { get; set; } = null!;

    public string? Icon { get; set; }

    public string? BaseUrl { get; set; }

    public virtual ICollection<SocialMediaPublishingLogPlatform> PublishingLogPlatforms { get; set; } = new List<SocialMediaPublishingLogPlatform>();
}
