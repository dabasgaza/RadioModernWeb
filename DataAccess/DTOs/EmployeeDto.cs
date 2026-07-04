// ============================================================
// EmployeeDto — الموظف
// ============================================================
// المسؤولية: تعريف الموظف.
// ============================================================
namespace DataAccess.DTOs;

/// <summary>
/// سجل الموظف.
/// </summary>
public record EmployeeDto(
    int EmployeeId,
    string FullName,
    int? StaffRoleId,
    string? StaffRoleName,
    string? Notes);