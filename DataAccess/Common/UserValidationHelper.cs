using Domain.Identity;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Common;

public static class UserValidationHelper
{
    public static async Task<bool> EnsureDomainUserExistsAsync(this BroadcastWorkflowDBContext context, UserSession session)
    {
        if (await context.Users.AnyAsync(u => u.UserId == session.UserId))
            return true;

        var domainUser = await context.Users
            .FirstOrDefaultAsync(u => u.Username == session.Username && u.IsActive);

        if (domainUser == null)
            return false;

        var appUser = await context.Set<ApplicationUser>()
            .FirstOrDefaultAsync(u => u.UserName == session.Username);

        if (appUser != null)
        {
            appUser.DomainUserId = domainUser.UserId;
            appUser.DomainRoleId = domainUser.RoleId;
            await context.SaveChangesAsync();
        }

        session.UserId = domainUser.UserId;

        return true;
    }
}
