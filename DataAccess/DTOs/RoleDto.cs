// ============================================================
// RoleDto — الدور
// ============================================================
// المسؤولية: تعريف الدور.
// ============================================================
namespace DataAccess.DTOs
{
    /// <summary>
    /// سجل الدور.
    /// </summary>
    public record RoleDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string RoleDescription { get; set; } = string.Empty;
    }
}
