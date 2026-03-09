namespace PatientHealthRecord.API.Middleware;

/// <summary>
/// OWASP security headers middleware extension
/// Applies security headers to all responses
/// </summary>
public static class OwaspSecurityMiddleware
{
    public static IApplicationBuilder ConfigureOwaspSecurity(this IApplicationBuilder app)
        => app.Use(async (ctx, next) =>
        {
            var h = ctx.Response.Headers;
            h.Append("X-Content-Type-Options", "nosniff");
            h.Append("X-Frame-Options", "DENY");
            h.Append("X-XSS-Protection", "1; mode=block");
            h.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            h.Append("Content-Security-Policy", "default-src 'self'");
            h.Append("Referrer-Policy", "no-referrer");
            h.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
            await next();
        });
}
