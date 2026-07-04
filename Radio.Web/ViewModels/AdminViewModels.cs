// ============================================================
// AdminViewModels — الإدارة
// ============================================================
// المسؤولية: تعريف الإدارة.
// ============================================================
using DataAccess.DTOs;

namespace Radio.Web.ViewModels;

/// <summary>
/// صنف المستخدم.
/// </summary>
public class UserViewModel
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Password { get; set; }

    /// <summary>
    /// To Dto.
    /// </summary>
    public UserDto ToDto() => new()
    {
        UserId = UserId,
        FullName = FullName,
        Username = Username,
        EmailAddress = EmailAddress,
        PhoneNumber = PhoneNumber,
        RoleId = RoleId,
        IsActive = IsActive
    };
}

/// <summary>
/// صنف الصلاحية المصفوفة.
/// </summary>
public class PermissionMatrixViewModel
{
    public List<RoleDto> Roles { get; set; } = new();
    public List<PermissionViewModel> Permissions { get; set; } = new();
    public int SelectedRoleId { get; set; }
    public string? SelectedRoleName { get; set; }
}
