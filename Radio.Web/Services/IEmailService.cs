namespace Radio.Web.Services;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, bool isHtml = false);
    Task SendBulkAsync(IEnumerable<string> recipients, string subject, string body);
    bool IsConfigured { get; }
}
