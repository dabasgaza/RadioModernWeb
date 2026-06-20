using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Radio.Web.Hubs;

/// <summary>
/// Hub الإشعارات اللحظية — يربط الـ Services بالواجهة عبر WebSocket.
/// كل عميل (متصفح) يتصل بهذا الـ Hub ويستقبل الإشعارات المُبثّة (تُعرض عبر Toastr).
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var user = Context.User?.Identity?.Name ?? "anonymous";
        _logger.LogInformation("SignalR: اتصال جديد - {User} ({ConnectionId})", user, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
            _logger.LogWarning(exception, "SignalR: انقطاع اتصال غير طبيعي - {ConnectionId}", Context.ConnectionId);
        else
            _logger.LogInformation("SignalR: انقطاع اتصال طبيعي - {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
