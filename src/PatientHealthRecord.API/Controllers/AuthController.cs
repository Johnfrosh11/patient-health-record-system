using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientHealthRecord.Application.Common.Interfaces;
using PatientHealthRecord.Application.DTOs.Auth;
using PatientHealthRecord.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace PatientHealthRecord.API.Controllers;

[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshTokenValidator;

    public AuthController(
        IAuthService authService,
        IAuditService auditService,
        IValidator<LoginRequest> loginValidator,
        IValidator<RegisterRequest> registerValidator,
        IValidator<RefreshTokenRequest> refreshTokenValidator)
    {
        _authService = authService;
        _auditService = auditService;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
        _refreshTokenValidator = refreshTokenValidator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Register a new user account with username, email, and password")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var validationResult = await _registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return NotifyModelStateError();
        }

        var response = await _authService.RegisterAsync(request);
        return Response(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Authenticate user and return JWT access and refresh tokens")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return NotifyModelStateError();
        }

        var response = await _authService.LoginAsync(request);
        
        await _auditService.LogAsync(AuditAction.Login, "User", response.UserId.ToString(), $"User {request.Username} logged in successfully");
        
        return Response(response);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Refresh expired access token using a valid refresh token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var validationResult = await _refreshTokenValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return NotifyModelStateError();
        }

        var response = await _authService.RefreshTokenAsync(request);
        return Response(response);
    }

    [HttpPost("logout")]
    [SwaggerOperation(Summary = "Logout user and revoke refresh token")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var validationResult = await _refreshTokenValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return NotifyModelStateError();
        }

        await _authService.LogoutAsync(request.RefreshToken);
        
        await _auditService.LogAsync(AuditAction.Logout, "User", User.FindFirst(ClaimTypes.NameIdentifier)?.Value, "User logged out");
        
        return Response(new { message = "Logged out successfully" });
    }

    [HttpGet("me")]
    [SwaggerOperation(Summary = "Get currently authenticated user's profile information")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var response = await _authService.GetCurrentUserAsync(userId);
        return Response(response);
    }
}
