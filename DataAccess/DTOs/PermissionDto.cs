// ============================================================
// PermissionDto — الصلاحية
// ============================================================
// المسؤولية: تعريف الصلاحية.
// ============================================================
namespace DataAccess.DTOs
{
    /// <summary>
    /// سجل الصلاحية.
    /// </summary>
    public record PermissionDto(
        int PermissionId,
        string SystemName,
        string DisplayName,
        string Module);

    /// <summary>
    /// سجل الصلاحية Upsert.
    /// </summary>
    public record PermissionUpsertDto(
        string SystemName,
        string DisplayName,
        string Module);
}
