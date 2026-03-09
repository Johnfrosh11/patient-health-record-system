using Microsoft.AspNetCore.Diagnostics;
using PatientHealthRecord.Utilities;

namespace PatientHealthRecord.API.Middleware;

/// <summary>
/// Global exception handler - catches all unhandled exceptions
/// NEVER expose internal error details to clients
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext ctx, Exception ex, CancellationToken ct)
    {
        // Log full exception internally — structured, with correlation ID
        logger.LogError(ex,
            "Unhandled exception. CorrelationId: {CorrelationId} | Path: {Path} | Method: {Method}",
            ctx.TraceIdentifier, ctx.Request.Path, ctx.Request.Method);

        // Return clean message — NEVER expose ex.Message or stack trace to client
        ctx.Response.StatusCode = 500;
        await ctx.Response.WriteAsJsonAsync(new ResponseModel<object>
        {
            code = "500",
            message = "An unexpected error occurred. Please try again or contact support.",
            success = false
        }, ct);

        return true;
    }
}
