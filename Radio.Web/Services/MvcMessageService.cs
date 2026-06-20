using System.Text.Json;
using DataAccess.Services.Messaging;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Radio.Web.Services;

/// <summary>
/// تنفيذ IMessageService لبيئة MVC — يستخدم TempData للتنبيهات عبر Redirects.
/// الـ Notifications تُعرض عبر Toastr.js (يقرأها من TempData في _Toastr.cshtml).
/// </summary>
public class MvcMessageService : IMessageService
{
    private readonly ITempDataDictionaryFactory _tempDataFactory;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ILogger<MvcMessageService> _logger;

    public MvcMessageService(
        ITempDataDictionaryFactory tempDataFactory,
        IHttpContextAccessor httpContext,
        ILogger<MvcMessageService> logger)
    {
        _tempDataFactory = tempDataFactory;
        _httpContext = httpContext;
        _logger = logger;
    }

    private void Push(string type, string message)
    {
        var ctx = _httpContext.HttpContext;
        if (ctx == null) return;

        var tempData = _tempDataFactory.GetTempData(ctx);
        var list = new List<ToastrMessage>();

        if (tempData.TryGetValue("ToastrNotifications", out var existing) && existing is string json)
        {
            try { list = JsonSerializer.Deserialize<List<ToastrMessage>>(json) ?? new(); }
            catch { list = new(); }
        }

        list.Add(new ToastrMessage { Type = type, Message = message });
        tempData["ToastrNotifications"] = JsonSerializer.Serialize(list);
        tempData.Save();
    }

    public void ShowSuccess(string message, string title = "نجاح")
    {
        _logger.LogInformation("✓ {Title}: {Message}", title, message);
        Push("success", message);
    }

    public void ShowError(string message, string title = "خطأ")
    {
        _logger.LogError("✗ {Title}: {Message}", title, message);
        Push("error", message);
    }

    public void ShowWarning(string message, string title = "تحذير")
    {
        _logger.LogWarning("⚠ {Title}: {Message}", title, message);
        Push("warning", message);
    }

    public void ShowInfo(string message, string title = "معلومة")
    {
        _logger.LogInformation("ℹ {Title}: {Message}", title, message);
        Push("info", message);
    }

    public Task<bool> ShowConfirmationAsync(string message, string title = "تأكيد")
    {
        // In MVC, confirmations are handled client-side via SweetAlert2
        return Task.FromResult(true);
    }
}

public class ToastrMessage
{
    public string Type { get; set; } = "info";
    public string Message { get; set; } = "";
}
