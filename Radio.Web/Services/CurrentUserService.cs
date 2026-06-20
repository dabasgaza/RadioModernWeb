using System.Security.Claims;
using DataAccess.Common;

namespace Radio.Web.Services;

public interface ICurrentUserService
{
    ClaimsPrincipal? User { get; }
    bool IsAuthenticated { get; }
    int DomainUserId { get; }
    int DomainRoleId { get; }
    string? UserName { get; }
    string? FullName { get; }
    string? PrimaryRole { get; }
    IReadOnlyList<string> Permissions { get; }
    UserSession? ToUserSession();
    bool HasPermission(string permissionName);
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContext;

    public CurrentUserService(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext;
    }

    public ClaimsPrincipal? User => _httpContext.HttpContext?.User;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
    public int DomainUserId => int.TryParse(User?.FindFirstValue("DomainUserId"), out var id) ? id : 0;
    public int DomainRoleId => int.TryParse(User?.FindFirstValue("DomainRoleId"), out var id) ? id : 0;
    public string? UserName => User?.FindFirstValue(ClaimTypes.Name);
    public string? FullName => User?.FindFirstValue("FullName");
    public string? PrimaryRole => User?.FindFirstValue(ClaimTypes.Role);

    public IReadOnlyList<string> Permissions =>
        User?.FindAll("Permission").Select(c => c.Value).ToList() ?? new List<string>();

    public bool HasPermission(string permissionName)
    {
        if (!IsAuthenticated) return false;
        if (User?.HasClaim(c => c.Type == "SuperAdmin") == true) return true;
        return Permissions.Contains(permissionName);
    }

    public UserSession? ToUserSession()
    {
        if (!IsAuthenticated) return null;
        return new UserSession
        {
            UserId = DomainUserId,
            Username = UserName ?? string.Empty,
            FullName = FullName ?? string.Empty,
            RoleName = PrimaryRole ?? "Unknown",
            Permissions = Permissions.ToList()
        };
    }
}

