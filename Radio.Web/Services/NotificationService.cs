using DataAccess.Services;
using Microsoft.AspNetCore.SignalR;

namespace Radio.Web.Services;

/// <summary>
/// خدمة الإشعارات المركزية — تبث الإشعارات عبر SignalR لجميع العملاء المتصلين.
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

    public async Task BroadcastAsync(string type, string title, string message, string? payload = null)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("NotificationReceived", new
            {
                Type = type,
                Title = title,
                Message = message,
                Payload = payload,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "تعذّر بث الإشعار عبر SignalR");
        }
    }

    public async Task NotifyEpisodeStatusChangedAsync(int episodeId, byte newStatusId)
        => await BroadcastAsync("EpisodeStatusChanged", "تحديث حالة الحلقة",
            $"تم تحديث حالة الحلقة #{episodeId} إلى الحالة {newStatusId}", episodeId.ToString());

    public async Task NotifyAuditLogCreatedAsync(string tableName, string action)
        => await BroadcastAsync("AuditLogCreated", "سجل تدقيق جديد",
            $"عملية {action} على {tableName}", null);
}
