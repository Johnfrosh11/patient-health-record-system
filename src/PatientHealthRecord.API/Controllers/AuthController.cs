using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientHealthRecord.Application.DTOs.Auth;
using PatientHealthRecord.Application.Services.Auth;
using PatientHealthRecord.Utilities;

namespace PatientHealthRecord.API.Controllers;

/// <summary>
/// Authentication controller - sealed, thin actions only
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
public sealed class AuthController(IAuthService svc) : BaseController
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto model, CancellationToken ct = default)
        => Response(await svc.LoginAsync(model, ct));

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto model, CancellationToken ct = default)
        => Response(await svc.RegisterAsync(model, ct));

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto model, CancellationToken ct = default)
        => Response(await svc.RefreshTokenAsync(model, ct));

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct = default)
        => Response(await svc.LogoutAsync(ct));
}
