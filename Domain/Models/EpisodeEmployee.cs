// ============================================================
// EpisodeEmployee — حلقة-موظف
// ============================================================
// المسؤولية: تعريف حلقة-موظف.
// ============================================================
namespace Domain.Models;

/// <summary>
/// صنف حلقة-موظف.
/// </summary>
public class EpisodeEmployee : BaseEntity
{
    public int EpisodeEmployeeId { get; set; }

    public int EpisodeId { get; set; }
    public int EmployeeId { get; set; }

    public virtual Episode Episode { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
}
