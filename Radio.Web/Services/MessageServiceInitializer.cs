using DataAccess.Services.Messaging;

namespace Radio.Web.Services;

public class MessageServiceInitializer : IHostedService
{
    private readonly IServiceProvider _sp;
    public MessageServiceInitializer(IServiceProvider sp) => _sp = sp;
    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _sp.CreateScope();
        var msg = scope.ServiceProvider.GetRequiredService<IMessageService>();
        MessageService.Initialize(msg);
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
