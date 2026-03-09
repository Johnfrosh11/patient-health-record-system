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

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// ── Serilog (before everything else) ─────────────────────────────────
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId()
    .WriteTo.Console()
    .WriteTo.File("logs/patient-health-record-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31));

// ── Configuration ─────────────────────────────────────────────────────
builder.Services.Configure<GlobalSettings>(config.GetSection("GlobalSettings"));
builder.Services.Configure<JwtSettings>(config.GetSection("JwtSettings"));

// ── Database ──────────────────────────────────────────────────────────
// Railway injects DATABASE_URL as a postgresql:// URI; fall back to appsettings for local dev.
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var connectionString = !string.IsNullOrEmpty(databaseUrl)
    ? databaseUrl                                      // Npgsql 6+ accepts postgresql:// URIs natively
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
            ClockSkew = TimeSpan.Zero, // zero tolerance on expiry
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

// ── Permission Authorization Handler ──────────────────────────────────
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

// ── Global Exception Handler ──────────────────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ── Rate Limiting ─────────────────────────────────────────────────────
builder.Services.AddRateLimitingPolicies();

// ── Application Services ──────────────────────────────────────────────
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

// ══ MIDDLEWARE PIPELINE (ORDER IS NON-NEGOTIABLE) ═════════════════════
app.UseExceptionHandler(opt => { }); // 1. Global exceptions first
app.ConfigureOwaspSecurity();         // 2. OWASP security headers

// Swagger — enabled in Development and when explicitly set (e.g. Railway staging)
if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("ENABLE_SWAGGER") == "true")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Patient Health Record API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseRouting();                     // 4. Routing
app.UseCors();                        // 5. CORS
app.UseHttpsRedirection();            // 6. HTTPS
app.UseSerilogRequestLogging();       // 7. Request logging
app.UseAuthentication();              // 8. Who are you?
app.UseAuthorization();               // 9. What can you do?
app.UseRateLimiter();                 // 10. Rate limit
app.MapControllers();                 // 11. Endpoints
app.MapHealthChecks("/health");        // 12. Health probe for Railway

// ── Database initialization ───────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<PatientHealthRecordDbContext>();
        await db.Database.MigrateAsync();   // applies any pending migrations
        await DatabaseSeeder.SeedAsync(services);
        Log.Information("Database seeded successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while seeding the database");
    }
}

try
{
    Log.Information("Starting Patient Health Record API");
    // Railway injects PORT; honour it so the reverse proxy can reach the container.
    var port = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrEmpty(port))
        app.Run($"http://0.0.0.0:{port}");
    else
        app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for WebApplicationFactory in integration tests
public partial class Program { }
