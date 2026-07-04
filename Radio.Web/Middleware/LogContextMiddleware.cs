// ============================================================
// LogContextMiddleware — سجل السياق Middleware
// ============================================================
// المسؤولية: تعريف سجل السياق Middleware.
// ============================================================
using Serilog.Context;

namespace Radio.Web.Middleware;

/// <summary>
/// صنف سجل السياق Middleware.
/// </summary>
public class LogContextMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Invoke Async.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        using (LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst("DomainUserId")?.Value;
                var userName = context.User.Identity.Name;

                using (LogContext.PushProperty("UserId", userId))
                using (LogContext.PushProperty("UserName", userName))
                {
                    await next(context);
                }
            }
            else
            {
                await next(context);
            }
        }
    }
}
