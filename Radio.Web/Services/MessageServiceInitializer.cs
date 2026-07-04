// ============================================================
// MessageServiceInitializer — مهيئ الرسائل
// ============================================================
// المسؤولية: تعريف مهيئ الرسائل.
// ============================================================
using DataAccess.Services.Messaging;

namespace Radio.Web.Services;

/// <summary>
/// صنف مهيئ الرسائل.
/// </summary>
public class MessageServiceInitializer : IHostedService
{
    private readonly IServiceProvider _sp;
    /// <summary>
    /// تهيئة مهيئ الرسائل.
    /// </summary>
    public MessageServiceInitializer(IServiceProvider sp) => _sp = sp;
    /// <summary>
    /// بدء Async.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _sp.CreateScope();
        var msg = scope.ServiceProvider.GetRequiredService<IMessageService>();
        MessageService.Initialize(msg);
        return Task.CompletedTask;
    }
    /// <summary>
    /// إيقاف Async.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
