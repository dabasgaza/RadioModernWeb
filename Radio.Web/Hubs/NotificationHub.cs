// ============================================================
// NotificationHub — Hub الإشعارات
// ============================================================
// المسؤولية: تعريف Hub الإشعارات.
// ============================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Radio.Web.Hubs;

/// <summary>
/// صنف Hub الإشعارات.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// عند Connected Async.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var user = Context.User?.Identity?.Name ?? "anonymous";
        _logger.LogInformation("SignalR: اتصال جديد - {User} ({ConnectionId})", user, Context.ConnectionId);

        var userId = Context.User?.FindFirstValue("DomainUserId");
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
        }

        var roles = Context.User?.FindAll(ClaimsIdentity.DefaultRoleClaimType)
            .Select(c => c.Value) ?? Enumerable.Empty<string>();
        foreach (var role in roles)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role:{role}");
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// عند Disconnected Async.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
            _logger.LogWarning(exception, "SignalR: انقطاع اتصال غير طبيعي - {ConnectionId}", Context.ConnectionId);
        else
            _logger.LogInformation("SignalR: انقطاع اتصال طبيعي - {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
