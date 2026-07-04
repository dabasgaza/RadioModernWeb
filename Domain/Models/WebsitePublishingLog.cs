// ============================================================
// WebsitePublishingLog — سجل نشر الموقع
// ============================================================
// المسؤولية: تعريف سجل نشر الموقع.
// ============================================================
namespace Domain.Models;

/// <summary>
/// صنف سجل نشر الموقع.
/// </summary>
public class WebsitePublishingLog : BaseEntity
{
    public int WebsitePublishingLogId { get; set; }

    public int EpisodeId { get; set; }
    public int PublishedByUserId { get; set; }

    public MediaType MediaType { get; set; }
    public string? Title { get; set; }
    public string? Notes { get; set; }

    public DateTime PublishedAt { get; set; }

    public virtual Episode Episode { get; set; } = null!;
    public virtual Domain.Identity.ApplicationUser PublishedByUser { get; set; } = null!;
}
