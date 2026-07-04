// ============================================================
// UserValidationHelper — التحقق من المستخدم
// ============================================================
// المسؤولية: تعريف التحقق من المستخدم.
// ============================================================
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Common;

/// <summary>
/// صنف التحقق من المستخدم.
/// </summary>
public static class UserValidationHelper
{
    /// <summary>
    /// تأكيد Domain المستخدم Exists Async.
    /// </summary>
    public static async Task<bool> EnsureDomainUserExistsAsync(this BroadcastWorkflowDBContext context, UserSession session)
    {
        return await context.Users.AnyAsync(u => u.Id == session.UserId && u.IsActive);
    }
}
