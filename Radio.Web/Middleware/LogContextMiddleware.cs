using Serilog.Context;

namespace Radio.Web.Middleware;

public class LogContextMiddleware(RequestDelegate next)
{
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
