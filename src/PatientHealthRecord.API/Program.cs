using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PatientHealthRecord.API.Authorization;
using PatientHealthRecord.API.Middleware;
using PatientHealthRecord.Application;
using PatientHealthRecord.Repository;
using PatientHealthRecord.Repository.Seed;
using PatientHealthRecord.Utilities;
using Serilog;

// Write to stderr immediately so Railway logs show something even if Serilog fails to init.
Console.Error.WriteLine("[startup] Patient Health Record API process starting...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── PORT — must be first so Kestrel binds even if later setup throws ──
    var railwayPort = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrEmpty(railwayPort))
    {
        builder.WebHost.UseUrls($"http://0.0.0.0:{railwayPort}");
        Console.Error.WriteLine($"[startup] Binding to PORT={railwayPort}");
    }

    var config = builder.Configuration;

    // ── Serilog ────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithCorrelationId()
        .WriteTo.Console()
        .WriteTo.File("logs/patient-health-record-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 31));

    // ── Configuration ──────────────────────────────────────────────────────
    builder.Services.Configure<GlobalSettings>(config.GetSection("GlobalSettings"));
    builder.Services.Configure<JwtSettings>(config.GetSection("JwtSettings"));

    // ── Database ───────────────────────────────────────────────────────────
    // Railway injects DATABASE_URL as a postgresql:// URI; convert it to key=value format for Npgsql.
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    var connectionString = !string.IsNullOrEmpty(databaseUrl)
        ? ConvertDatabaseUrl(databaseUrl)
        : config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");

    builder.Services.AddDbContext<PatientHealthRecordDbContext>(options =>
        options.UseNpgsql(connectionString,
            npgsql => npgsql.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), null)
                            .MigrationsAssembly("PatientHealthRecord.Repository")));

    // ── Authentication ─────────────────────────────────────────────────────
    var jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>()
        ?? throw new InvalidOperationException("JWT settings not configured.");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireSignedTokens = true,
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = ctx =>
                {
                    if (ctx.Exception is SecurityTokenExpiredException)
                        ctx.Response.Headers.Append("Token-Expired", "true");
                    return Task.CompletedTask;
                },
                OnChallenge = async ctx =>
                {
                    ctx.HandleResponse();
                    ctx.Response.StatusCode = 401;
                    await ctx.Response.WriteAsJsonAsync(new ResponseModel<object>
                    {
                        code = "401",
                        message = "Unauthorized. Please login.",
                        success = false
                    });
                }
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPermissionPolicies();
    });

    // ── Permission Authorization Handler ───────────────────────────────────
    builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

    // ── Global Exception Handler ───────────────────────────────────────────
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    // ── Rate Limiting ──────────────────────────────────────────────────────
    builder.Services.AddRateLimitingPolicies();

    // ── Application Services ───────────────────────────────────────────────
    builder.Services.InitServices(); // From AppBootstrapper

    // ── Health Checks (Railway uses /health to detect readiness) ──────────
    builder.Services.AddHealthChecks();

    // ── API + OpenAPI ──────────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Patient Health Record API",
            Version = "v1",
            Description = "Enterprise-grade Patient Health Record System with RBAC and time-bound access control"
        });

        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "JWT Authorization header. Enter your token ONLY."
        });

        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // ── CORS — never wildcard in production ───────────────────────────────
    var corsOrigins = config.GetSection("CorsOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };
    builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()));

    var app = builder.Build();

    // ══ MIDDLEWARE PIPELINE ════════════════════════════════════════════════
    app.UseExceptionHandler(opt => { }); // 1. Global exceptions first
    app.ConfigureOwaspSecurity();         // 2. OWASP security headers

    // Swagger — enabled in Development and when explicitly set
    if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("ENABLE_SWAGGER") == "true")
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Patient Health Record API v1");
            c.RoutePrefix = string.Empty;
        });
    }

    app.UseRouting();
    app.UseCors();
    // TLS is terminated at Railway's edge proxy; HTTPS redirect is only needed locally.
    if (app.Environment.IsDevelopment())
        app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.MapControllers();
    app.MapHealthChecks("/health"); // Railway readiness probe

    Log.Information("Starting Patient Health Record API");

    // Start listening FIRST — Railway probes /health immediately after the container starts,
    // so the app must accept connections before migrations run.
    await app.StartAsync();
    Log.Information("Kestrel started — now running database migrations");

    // ── Database initialization (app already responding to /health) ────────
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var db = services.GetRequiredService<PatientHealthRecordDbContext>();
            await db.Database.MigrateAsync();
            await DatabaseSeeder.SeedAsync(services);
            Log.Information("Database migrations and seed completed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Database migration/seed failed — app will continue running");
        }
    }

    await app.WaitForShutdownAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[FATAL] Application terminated unexpectedly: {ex}");
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for WebApplicationFactory in integration tests
public partial class Program
{
    // Converts postgresql://user:pass@host:port/db  →  Host=host;Port=port;Database=db;Username=user;Password=pass
    static string ConvertDatabaseUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var userInfo = uri.UserInfo.Split(':', 2);
            var username = Uri.UnescapeDataString(userInfo[0]);
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');
            return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        }
        catch
        {
            // Already a key=value string — return as-is
            return url;
        }
    }
}
