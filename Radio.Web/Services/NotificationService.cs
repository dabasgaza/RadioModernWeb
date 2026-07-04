// ============================================================
// NotificationService — الإشعارات
// ============================================================
// المسؤولية: تعريف الإشعارات.
// ============================================================
using Microsoft.AspNetCore.SignalR;

namespace Radio.Web.Services;

/// <summary>
/// صنف الإشعارات.
/// </summary>
public class NotificationService
{
    private readonly IHubContext<Hubs.NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<Hubs.NotificationHub> hubContext, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// بناء Payload.
    /// </summary>
    private object BuildPayload(string type, string title, string message, string? payload = null) => new
    {
        Type = type,
        Title = title,
        Message = message,
        Payload = payload,
        Timestamp = DateTime.UtcNow
    };

    /// <summary>
    /// Broadcast Async.
    /// </summary>
    public async Task BroadcastAsync(string type, string title, string message, string? payload = null)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("NotificationReceived", BuildPayload(type, title, message, payload));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "تعذّر بث الإشعار عبر SignalR");
        }
    }

    /// <summary>
    /// إرسال To المستخدم Async.
    /// </summary>
    public async Task SendToUserAsync(int userId, string type, string title, string message, string? payload = null)
    {
        try
        {
            await _hubContext.Clients.Group($"user:{userId}")
                .SendAsync("NotificationReceived", BuildPayload(type, title, message, payload));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "تعذّر إرسال الإشعار إلى المستخدم {UserId}", userId);
        }
    }

    /// <summary>
    /// إرسال To الدور Async.
    /// </summary>
    public async Task SendToRoleAsync(string role, string type, string title, string message, string? payload = null)
    {
        try
        {
            await _hubContext.Clients.Group($"role:{role}")
                .SendAsync("NotificationReceived", BuildPayload(type, title, message, payload));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "تعذّر إرسال الإشعار إلى دور {Role}", role);
        }
    }

    /// <summary>
    /// إرسال إشعار الحلقة الحالة Changed Async.
    /// </summary>
    public async Task NotifyEpisodeStatusChangedAsync(int episodeId, byte newStatusId)
        => await BroadcastAsync("EpisodeStatusChanged", "تحديث حالة الحلقة",
            $"تم تحديث حالة الحلقة #{episodeId} إلى الحالة {newStatusId}", episodeId.ToString());

    /// <summary>
    /// إرسال إشعار التدقيق سجل Created Async.
    /// </summary>
    public async Task NotifyAuditLogCreatedAsync(string tableName, string action)
        => await BroadcastAsync("AuditLogCreated", "سجل تدقيق جديد",
            $"عملية {action} على {tableName}", null);
}
