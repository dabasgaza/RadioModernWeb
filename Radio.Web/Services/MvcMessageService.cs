// ============================================================
// MvcMessageService — رسائل MVC
// ============================================================
// المسؤولية: تعريف رسائل MVC.
// ============================================================
using DataAccess.Services.Messaging;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Json;

namespace Radio.Web.Services;

/// <summary>
/// تنفيذ IMessageService لبيئة MVC — يستخدم TempData للتنبيهات عبر Redirects.
/// الـ Notifications تُعرض عبر Toastr.js (يقرأها من TempData في _Toastr.cshtml).
/// <summary>
/// صنف رسائل MVC.
/// </summary>
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

    /// <summary>
    /// Push.
    /// </summary>
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

    /// <summary>
    /// Show Success.
    /// </summary>
    public void ShowSuccess(string message, string title = "نجاح")
    {
        _logger.LogInformation("✓ {Title}: {Message}", title, message);
        Push("success", message);
    }

    /// <summary>
    /// Show Error.
    /// </summary>
    public void ShowError(string message, string title = "خطأ")
    {
        _logger.LogError("✗ {Title}: {Message}", title, message);
        Push("error", message);
    }

    /// <summary>
    /// Show Warning.
    /// </summary>
    public void ShowWarning(string message, string title = "تحذير")
    {
        _logger.LogWarning("⚠ {Title}: {Message}", title, message);
        Push("warning", message);
    }

    /// <summary>
    /// Show معلومات.
    /// </summary>
    public void ShowInfo(string message, string title = "معلومة")
    {
        _logger.LogInformation("ℹ {Title}: {Message}", title, message);
        Push("info", message);
    }

    /// <summary>
    /// Show Confirmation Async.
    /// </summary>
    public Task<bool> ShowConfirmationAsync(string message, string title = "تأكيد", CancellationToken cancellationToken = default)
    {
        // In MVC, confirmations are handled client-side via SweetAlert2
        return Task.FromResult(true);
    }
}

/// <summary>
/// صنف Toastr الرسالة.
/// </summary>
public class ToastrMessage
{
    public string Type { get; set; } = "info";
    public string Message { get; set; } = string.Empty;
}
