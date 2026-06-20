using System.Security.Claims;
using DataAccess.Common;
using DataAccess.DTOs;
using DataAccess.DTOs;
using DataAccess.Services;

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
        if (string.Equals(PrimaryRole, "Admin", StringComparison.OrdinalIgnoreCase)) return true;
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

/// <summary>
/// خدمة بيانات العرض — توفر بيانات مشتركة لكل الصفحات (Sidebar, Lookup data, etc.)
/// </summary>
public interface IViewDataService
{
    Task<List<Domain.Models.Program>> GetActiveProgramsAsync();
    Task<Dictionary<byte, string>> GetEpisodeStatusesAsync();
    Task<List<StaffRoleDto>> GetStaffRolesAsync();
    Task<List<SocialMediaPlatformDto>> GetSocialPlatformsAsync();
}

public class ViewDataService : IViewDataService
{
    private readonly ICachedLookupService _lookup;
    public ViewDataService(ICachedLookupService lookup) => _lookup = lookup;

    public Task<List<Domain.Models.Program>> GetActiveProgramsAsync() =>
        _lookup.GetProgramsAsync().ContinueWith(t => t.Result
            .Select(p => new Domain.Models.Program { ProgramId = p.ProgramId, ProgramName = p.ProgramName, Category = p.Category ?? "", ProgramDescription = p.ProgramDescription ?? "" })
            .ToList());

    public Task<Dictionary<byte, string>> GetEpisodeStatusesAsync() => _lookup.GetEpisodeStatusesAsync();
    public Task<List<StaffRoleDto>> GetStaffRolesAsync() => _lookup.GetStaffRolesAsync();
    public Task<List<SocialMediaPlatformDto>> GetSocialPlatformsAsync() => _lookup.GetSocialPlatformsAsync();
}
