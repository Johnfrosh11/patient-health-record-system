using System.Diagnostics;
using System.Security.Claims;

namespace PatientHealthRecord.API.Middleware;

public class AuditLoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggerMiddleware> _logger;

    public AuditLoggerMiddleware(RequestDelegate next, ILogger<AuditLoggerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();

        LogRequest(context, requestId);

        var originalBodyStream = context.Response.Body;

        try
        {
            await _next(context);
            stopwatch.Stop();

            LogResponse(context, requestId, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogException(context, requestId, ex, stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private void LogRequest(HttpContext context, string requestId)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
        var userEmail = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "N/A";
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value ?? "N/A";

        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = context.Request.Headers["User-Agent"].ToString();

        _logger.LogInformation(
            "[AUDIT] Request | RequestId: {RequestId} | Method: {Method} | Path: {Path} | " +
            "UserId: {UserId} | Email: {Email} | Role: {Role} | IP: {IpAddress} | UserAgent: {UserAgent} | QueryString: {QueryString}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            userId,
            userEmail,
            userRole,
            ipAddress,
            userAgent,
            context.Request.QueryString.ToString()
        );
    }

    private void LogResponse(HttpContext context, string requestId, long elapsedMs)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";

        _logger.LogInformation(
            "[AUDIT] Response | RequestId: {RequestId} | Method: {Method} | Path: {Path} | " +
            "UserId: {UserId} | StatusCode: {StatusCode} | Duration: {Duration}ms",
            requestId,
            context.Request.Method,
            context.Request.Path,
            userId,
            context.Response.StatusCode,
            elapsedMs
        );
    }

    private void LogException(HttpContext context, string requestId, Exception ex, long elapsedMs)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";

        _logger.LogError(ex,
            "[AUDIT] Exception | RequestId: {RequestId} | Method: {Method} | Path: {Path} | " +
            "UserId: {UserId} | ExceptionType: {ExceptionType} | Duration: {Duration}ms",
            requestId,
            context.Request.Method,
            context.Request.Path,
            userId,
            ex.GetType().Name,
            elapsedMs
        );
    }
}
