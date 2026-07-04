// ============================================================
// StaffRole — المسمى الوظيفي
// ============================================================
// المسؤولية: تعريف المسمى الوظيفي.
// ============================================================
namespace Domain.Models;

/// <summary>
/// صنف المسمى الوظيفي.
/// </summary>
public class StaffRole : BaseEntity
{
    public int StaffRoleId { get; set; }

    public string RoleName { get; set; } = null!; // e.g. مذيع, منفذ, مهندس صوت

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
