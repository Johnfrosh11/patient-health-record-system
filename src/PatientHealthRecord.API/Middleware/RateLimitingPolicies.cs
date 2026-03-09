using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace PatientHealthRecord.API.Middleware;

/// <summary>
/// Rate limiting policies configuration
/// </summary>
public static class RateLimitingPolicies
{
    public const string StandardPolicy = "standard";
    public const string AuthPolicy = "auth";

    public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Standard policy: 100 requests per minute per IP
            options.AddFixedWindowLimiter(StandardPolicy, opt =>
            {
                opt.PermitLimit = 100;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 10;
            });

            // Auth policy: 10 requests per minute per IP (stricter for auth endpoints)
            options.AddFixedWindowLimiter(AuthPolicy, opt =>
            {
                opt.PermitLimit = 10;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 2;
            });

            options.RejectionStatusCode = 429;
            options.OnRejected = async (context, ct) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    code = "429",
                    success = false,
                    message = "Too many requests. Please try again later."
                }, cancellationToken: ct);
            };
        });

        return services;
    }
}
