// ============================================================
// DatabaseHealthCheck — فحص صحة قاعدة البيانات
// ============================================================
// المسؤولية: تعريف فحص صحة قاعدة البيانات.
// ============================================================
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Radio.Web.HealthChecks;

/// <summary>
/// صنف فحص صحة قاعدة البيانات.
/// </summary>
public class DatabaseHealthCheck(
    IDbContextFactory<BroadcastWorkflowDBContext> contextFactory,
    ILogger<DatabaseHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
            var canConnect = await ctx.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
                return HealthCheckResult.Unhealthy("لا يمكن الاتصال بقاعدة البيانات");

            var episodeCount = await ctx.Episodes.CountAsync(cancellationToken);

            return HealthCheckResult.Healthy("قاعدة البيانات متصلة", new Dictionary<string, object>
            {
                { "episode_count", episodeCount },
                { "database", ctx.Database.GetConnectionString()?.GetHashCode() ?? 0 }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("فشل الاتصال بقاعدة البيانات", ex);
        }
    }
}
