// ============================================================
// CurrentSessionProvider — مزود الجلسة
// ============================================================
// المسؤولية: تعريف مزود الجلسة.
// ============================================================
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess.Common
{
    /// <summary>
    /// صنف مزود الجلسة.
    /// </summary>
    public class CurrentSessionProvider(IServiceProvider serviceProvider)
    {
        private static readonly AsyncLocal<UserSession?> _current = new();

        public UserSession? CurrentSession
        {
            get => _current.Value;
            set => _current.Value = value;
        }

        /// <summary>
        /// Refresh الصلاحيات Async.
        /// </summary>
        public async Task RefreshPermissionsAsync()
        {
            if (CurrentSession == null) return;

            var contextFactory = serviceProvider.GetRequiredService<IDbContextFactory<BroadcastWorkflowDBContext>>();
            await using var context = await contextFactory.CreateDbContextAsync();

            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == CurrentSession.UserId);

            if (user != null)
            {
                var role = await context.Roles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == user.RoleId);

                var permissions = await context.RoleClaims
                    .AsNoTracking()
                    .Where(rc => rc.RoleId == user.RoleId && rc.ClaimType == "Permission" && rc.ClaimValue != null)
                    .Select(rc => rc.ClaimValue!)
                    .ToListAsync();

                CurrentSession.RoleName = role?.Name ?? "Unknown";
                CurrentSession.Permissions = permissions;
            }
        }
    }
}
