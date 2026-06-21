using DataAccess.Common;
using DataAccess.DTOs;
using System.Threading;

namespace DataAccess.Services;

public interface IPlatformService
{
    Task<List<SocialMediaPlatformDto>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<Result<int>> CreateAsync(SocialMediaPlatformDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(SocialMediaPlatformDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int platformId, UserSession session, CancellationToken cancellationToken = default);
}

