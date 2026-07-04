// ============================================================
// GlobalExceptionHandler — Global Exception
// ============================================================
// المسؤولية: تعريف Global Exception.
// ============================================================
using DataAccess.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using System.Diagnostics;

namespace Radio.Web.Middleware;

/// <summary>
/// صنف Global Exception.
/// </summary>
public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IWebHostEnvironment env) : IExceptionHandler
{
    private static readonly ProblemDetailsFactory Factory = new();

    /// <summary>
    /// محاولة Handle Async.
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = Factory.CreateProblemDetails(exception, env);

        using (LogContext.PushProperty("CorrelationId", httpContext.TraceIdentifier))
        using (LogContext.PushProperty("ExceptionType", exception.GetType().Name))
        {
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }

        httpContext.Response.StatusCode = problemDetails.Status ?? 500;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    /// <summary>
    /// صنف Problem Details Factory.
    /// </summary>
    private class ProblemDetailsFactory
    {
        /// <summary>
        /// إنشاء Problem Details.
        /// </summary>
        public ProblemDetails CreateProblemDetails(Exception exception, IWebHostEnvironment env)
        {
            var isDev = env.IsDevelopment();

            var problem = new ProblemDetails
            {
                Title = "حدث خطأ في الخادم",
                Detail = isDev
                    ? exception.Message
                    : "عذراً، حدث خطأ غير متوقع. يرجى المحاولة لاحقاً.",
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Instance = "Error"
            };

            problem.Extensions["traceId"] = Activity.Current?.Id ?? "N/A";
            problem.Extensions["timestamp"] = DateTime.UtcNow;

            if (exception is ConcurrencyException concurrencyEx)
            {
                problem.Status = StatusCodes.Status409Conflict;
                problem.Title = "تعارض في تعديل البيانات";
                problem.Detail = "قام مستخدم آخر بتعديل هذه البيانات أثناء قيامك بالتحرير.";
                problem.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8";
                problem.Extensions["databaseValues"] = concurrencyEx.DatabaseValues;
            }

            if (isDev)
            {
                problem.Extensions["stackTrace"] = exception.StackTrace;
                if (exception.InnerException != null)
                    problem.Extensions["innerException"] = exception.InnerException.Message;
            }

            return problem;
        }
    }
}
