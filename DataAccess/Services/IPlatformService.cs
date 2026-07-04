// ============================================================
// IPlatformService — I Platform
// ============================================================
// المسؤولية: تعريف I Platform.
// ============================================================
using DataAccess.Common;
using DataAccess.DTOs;

namespace DataAccess.Services;

/// <summary>
/// واجهة I Platform.
/// </summary>
public interface IPlatformService
{
    Task<List<SocialMediaPlatformDto>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<Result<int>> CreateAsync(SocialMediaPlatformDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(SocialMediaPlatformDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int platformId, UserSession session, CancellationToken cancellationToken = default);
}

