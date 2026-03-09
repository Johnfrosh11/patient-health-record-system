using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PatientHealthRecord.Application.Auth;
using PatientHealthRecord.Application.Auth.Interface;
using PatientHealthRecord.Application.Services.AccessRequests;
using PatientHealthRecord.Application.Services.Audit;
using PatientHealthRecord.Application.Services.Auth;
using PatientHealthRecord.Application.Services.HealthRecords;
using PatientHealthRecord.Application.Services.Roles;
using PatientHealthRecord.Application.Services.Users;

namespace PatientHealthRecord.Application;

/// <summary>
/// Application layer dependency injection configuration
/// All service registrations live in this file
/// </summary>
public static class AppBootstrapper
{
    public static IServiceCollection InitServices(this IServiceCollection services)
    {
        // ── HTTP Context (required for IAuthUser) ─────────────────────
        services.AddHttpContextAccessor();

        // ── Auth User (Scoped — one per request) ──────────────────────
        services.AddScoped<IAuthUser, AuthUser>();

        // ── Feature Services (Scoped — one per request) ───────────────
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IHealthRecordService, HealthRecordService>();
        services.AddScoped<IAccessRequestService, AccessRequestService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuditService, AuditService>();

        // ── Infrastructure Services (Scoped) ──────────────────────────
        // Add external services here (email, SMS, blob storage, etc.)

        // ── Caching (Singleton — shared across requests) ───────────────
        // Single instance: in-memory cache
        // Multi-instance (Azure Container Apps): replace with Redis
        services.AddMemoryCache();

        // ── FluentValidation (auto-discovers all validators in assembly) ─
        // Use any non-static type from this assembly as marker
        services.AddValidatorsFromAssemblyContaining<AuthService>();

        return services;
    }
}

// Lifetime rules:
// Scoped    → Touches DbContext or HttpContext. One instance per HTTP request.
// Singleton → Shared state across all requests (cache, config, static data).
// Transient → Lightweight stateless utility. Rarely used.
