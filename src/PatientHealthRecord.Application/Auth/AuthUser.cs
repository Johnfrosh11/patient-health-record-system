using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PatientHealthRecord.Application.Auth.Interface;

namespace PatientHealthRecord.Application.Auth;

/// <summary>
/// AuthUser implementation - extracts user information from HTTP context claims
/// </summary>
public sealed class AuthUser(IHttpContextAccessor ctx) : IAuthUser
{
    private ClaimsPrincipal? User => ctx.HttpContext?.User;

    public Guid   UserId         => Guid.TryParse(Claim(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
    public Guid   OrganizationId => Guid.TryParse(Claim("OrganizationId"), out var orgId) ? orgId : Guid.Empty;
    public string Email          => Claim(ClaimTypes.Email) ?? string.Empty;
    public string FullName       => Claim(ClaimTypes.Name) ?? string.Empty;
    public string Username       => Claim("Username") ?? string.Empty;
    public string CorrelationId  => ctx.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
    public bool   Authenticated  => User?.Identity?.IsAuthenticated ?? false;
    public string IpAddress      => ctx.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private string? Claim(string type) => User?.FindFirstValue(type);
}
