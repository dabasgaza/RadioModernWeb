namespace Radio.Web.Services;

public class EmailService(ILogger<EmailService> logger, IConfiguration config) : IEmailService
{
    private readonly string _logDir = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "emails");

    public bool IsConfigured =>
        !string.IsNullOrEmpty(config["Email:Host"]) &&
        !string.IsNullOrEmpty(config["Email:Port"]);

    public async Task SendAsync(string to, string subject, string body, bool isHtml = false)
    {
        if (!IsConfigured)
        {
            await LogEmail(to, subject, body);
            return;
        }

        try
        {
            // TODO: Implement with MailKit or System.Net.Mail when configured
            // using var client = new SmtpClient();
            // await client.ConnectAsync(config["Email:Host"], int.Parse(config["Email:Port"]), true);
            // await client.AuthenticateAsync(config["Email:User"], config["Email:Password"]);
            // await client.SendAsync(...);
            await LogEmail(to, subject, body);
            logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To}", to);
        }
    }

    public async Task SendBulkAsync(IEnumerable<string> recipients, string subject, string body)
    {
        foreach (var to in recipients)
            await SendAsync(to, subject, body);
    }

    private async Task LogEmail(string to, string subject, string body)
    {
        Directory.CreateDirectory(_logDir);
        var file = Path.Combine(_logDir, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(file, $"To: {to}\nSubject: {subject}\nBody:\n{body}");
        logger.LogInformation("Email logged to {File}: {To} — {Subject}", file, to, subject);
    }
}
