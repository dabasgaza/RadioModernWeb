// ============================================================
// EpisodeCorrespondent — حلقة-مراسل
// ============================================================
// المسؤولية: تعريف حلقة-مراسل.
// ============================================================
namespace Domain.Models;

/// <summary>
/// صنف حلقة-مراسل.
/// </summary>
public class EpisodeCorrespondent : BaseEntity
{
    public int EpisodeCorrespondentId { get; set; }

    public int EpisodeId { get; set; }
    public int CorrespondentId { get; set; }

    public string? Topic { get; set; }
    public TimeSpan? HostingTime { get; set; }

    public virtual Episode Episode { get; set; } = null!;
    public virtual Correspondent Correspondent { get; set; } = null!;
}
