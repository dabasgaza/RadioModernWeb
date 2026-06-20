using DataAccess.Common;
using Radio.Web.Services;

namespace Radio.Web.Middleware
{
    public class SessionCaptureMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionCaptureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext ctx,
            CurrentSessionProvider provider,
            ICurrentUserService currentUser)
        {
            if (ctx.User?.Identity?.IsAuthenticated == true)
            {
                provider.CurrentSession = currentUser.ToUserSession();
            }
            await _next(ctx);
        }
    }
}
