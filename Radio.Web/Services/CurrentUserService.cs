// ============================================================
// CurrentUserService — المستخدم الحالي
// ============================================================
// المسؤولية: تعريف المستخدم الحالي.
// ============================================================
using DataAccess.Common;
using DataAccess.Services;
using System.Security.Claims;

namespace Radio.Web.Services;

/// <summary>
/// واجهة I Current المستخدم.
/// </summary>
public interface ICurrentUserService
{
    ClaimsPrincipal? User { get; }
    bool IsAuthenticated { get; }
    int DomainUserId { get; }
    int RoleId { get; }
    string? UserName { get; }
    string? FullName { get; }
    string? PrimaryRole { get; }
    IReadOnlyList<string> Permissions { get; }
    UserSession? ToUserSession();
    bool HasPermission(string permissionName);
}

/// <summary>
/// صنف المستخدم الحالي.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContext;
    private readonly IPermissionEvaluationService? _permissionEvaluation;

    public CurrentUserService(IHttpContextAccessor httpContext, IPermissionEvaluationService? permissionEvaluation = null)
    {
        _httpContext = httpContext;
        _permissionEvaluation = permissionEvaluation;
    }

    public ClaimsPrincipal? User => _httpContext.HttpContext?.User;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
    public int DomainUserId => int.TryParse(User?.FindFirstValue("DomainUserId"), out var id) ? id : 0;
    public int RoleId => int.TryParse(User?.FindFirstValue("DomainRoleId"), out var id) ? id : 0;
    public string? UserName => User?.FindFirstValue(ClaimTypes.Name);
    public string? FullName => User?.FindFirstValue("FullName");
    public string? PrimaryRole => User?.FindFirstValue(ClaimTypes.Role);

    public IReadOnlyList<string> Permissions =>
        User?.FindAll("Permission").Select(c => c.Value).ToList() ?? new List<string>();

    /// <summary>
    /// التحقق من الصلاحية باستخدام محرك التقييم الديناميكي.
    /// </summary>
    public bool HasPermission(string permissionName)
    {
        if (!IsAuthenticated) return false;
        if (User?.HasClaim(c => c.Type == "SuperAdmin") == true) return true;

        // استخدام محرك التقييم الديناميكي (يدعم التسوية + الاستثناءات الفردية)
        if (_permissionEvaluation != null && User != null)
        {
            return _permissionEvaluation.HasPermission(User, permissionName);
        }

        // Fallback: التحقق المباشر من الـ claims
        return Permissions.Contains(permissionName);
    }

    /// <summary>
    /// To المستخدم الجلسة.
    /// </summary>
    public UserSession? ToUserSession()
    {
        if (!IsAuthenticated) return null;
        var permissions = new List<string>();
        if (_permissionEvaluation != null && DomainUserId > 0 && RoleId > 0)
        {
            try
            {
                permissions = Task.Run(() => _permissionEvaluation.GetEffectivePermissionsAsync(DomainUserId, RoleId)).GetAwaiter().GetResult() ?? new();
            }
            catch
            {
                permissions = Permissions.ToList();
            }
        }
        else
        {
            permissions = Permissions.ToList();
        }
        return new UserSession
        {
            UserId = DomainUserId,
            Username = UserName ?? string.Empty,
            FullName = FullName ?? string.Empty,
            RoleName = PrimaryRole ?? "Unknown",
            Permissions = permissions
        };
    }
}

