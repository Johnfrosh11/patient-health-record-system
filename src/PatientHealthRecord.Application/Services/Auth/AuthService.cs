using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PatientHealthRecord.Application.Auth.Interface;
using PatientHealthRecord.Application.DTOs.Auth;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Repository;
using PatientHealthRecord.Utilities;

namespace PatientHealthRecord.Application.Services.Auth;

/// <summary>
/// Authentication service implementation - sealed, uses primary constructor
/// </summary>
public sealed class AuthService(
    PatientHealthRecordDbContext db,
    IAuthUser authUser,
    IOptions<JwtSettings> jwtOpts,
    ILogger<AuthService> logger
) : IAuthService
{
    // IOptions value assigned OUTSIDE constructor body (standard pattern)
    private readonly JwtSettings _JwtSettings = jwtOpts.Value;

    public async Task<ResponseModel<AuthViewModel>> LoginAsync(LoginDto model, CancellationToken ct = default)
    {
        try
        {
            // Step 1: Null check
            if (model == null) return Fail<AuthViewModel>("Request body is required.");

            // Step 2: Find user by username
            var user = await db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.IsActive, ct);

            if (user == null)
                return Fail<AuthViewModel>("Invalid username or password.");

            // Step 3: Verify password
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                logger.LogWarning("Failed login attempt for user {Username}", model.Username);
                return Fail<AuthViewModel>("Invalid username or password.");
            }

            // Step 4: Generate tokens
            var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();
            var permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.PermissionName)
                .Distinct()
                .ToList();

            var accessToken = GenerateAccessToken(user, roles, permissions);
            var refreshToken = GenerateRefreshToken();

            // Step 5: Save refresh token
            var tokenEntity = new TRefreshToken
            {
                UserId = user.UserId,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                OrganizationId = user.OrganizationId,
                CreatedBy = user.UserId.ToString(),
            };

            await db.RefreshTokens.AddAsync(tokenEntity, ct);
            await db.SaveChangesAsync(ct);

            // Step 6: Return success
            var authVm = new AuthViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                OrganizationId = user.OrganizationId,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddSeconds(_JwtSettings.JwtExpires),
                Roles = roles,
                Permissions = permissions
            };

            logger.LogInformation(
                "User {Username} logged in successfully. CorrelationId: {CorrelationId}",
                user.Username, authUser.CorrelationId);

            return Success("Login successful.", authVm);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login for {Username}", model?.Username);
            return Fail<AuthViewModel>("An error occurred during login.");
        }
    }

    public async Task<ResponseModel<AuthViewModel>> RegisterAsync(RegisterDto model, CancellationToken ct = default)
    {
        try
        {
            // Step 1: Null check
            if (model == null) return Fail<AuthViewModel>("Request body is required.");

            // Step 2: Check for duplicate username/email
            var exists = await db.Users.AnyAsync(
                u => (u.Username == model.Username || u.Email == model.Email) && u.IsActive, ct);

            if (exists)
                return Fail<AuthViewModel>("Username or email already exists.");

            // Step 3: Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password, 12);

            // Step 4: Create user entity
            var user = new TUser
            {
                Username = model.Username.Trim(),
                Email = model.Email.Trim().ToLower(),
                PasswordHash = passwordHash,
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                OrganizationId = model.OrganizationId,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow,
            };

            // Step 5: Persist
            await db.Users.AddAsync(user, ct);
            await db.SaveChangesAsync(ct);

            logger.LogInformation("New user registered: {Username}", user.Username);

            // Auto-login after registration
            return await LoginAsync(new LoginDto 
            { 
                Username = model.Username, 
                Password = model.Password 
            }, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error registering user {Username}", model?.Username);
            return Fail<AuthViewModel>("An error occurred during registration.");
        }
    }

    public async Task<ResponseModel<AuthViewModel>> RefreshTokenAsync(RefreshTokenDto model, CancellationToken ct = default)
    {
        try
        {
            if (model == null || string.IsNullOrWhiteSpace(model.RefreshToken))
                return Fail<AuthViewModel>("Refresh token is required.");

            var tokenEntity = await db.RefreshTokens
                .Include(rt => rt.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                            .ThenInclude(r => r.RolePermissions)
                                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(rt => rt.Token == model.RefreshToken && !rt.IsRevoked, ct);

            if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow)
                return Fail<AuthViewModel>("Invalid or expired refresh token.");

            var user = tokenEntity.User;
            var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();
            var permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.PermissionName)
                .Distinct()
                .ToList();

            // Generate new tokens
            var accessToken = GenerateAccessToken(user, roles, permissions);
            var newRefreshToken = GenerateRefreshToken();

            // Revoke old token
            tokenEntity.IsRevoked = true;
            tokenEntity.RevokedReason = "Refreshed";

            // Create new refresh token
            var newTokenEntity = new TRefreshToken
            {
                UserId = user.UserId,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                OrganizationId = user.OrganizationId,
                CreatedBy = user.UserId.ToString(),
            };

            db.RefreshTokens.Update(tokenEntity);
            await db.RefreshTokens.AddAsync(newTokenEntity, ct);
            await db.SaveChangesAsync(ct);

            var authVm = new AuthViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                OrganizationId = user.OrganizationId,
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddSeconds(_JwtSettings.JwtExpires),
                Roles = roles,
                Permissions = permissions
            };

            return Success("Token refreshed successfully.", authVm);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing token");
            return Fail<AuthViewModel>("An error occurred during token refresh.");
        }
    }

    public async Task<ResponseModel<bool>> LogoutAsync(CancellationToken ct = default)
    {
        try
        {
            if (!authUser.Authenticated)
                return Fail<bool>("User not authenticated.");

            // Revoke all active refresh tokens for the user
            var tokens = await db.RefreshTokens
                .Where(rt => rt.UserId == authUser.UserId && !rt.IsRevoked)
                .ToListAsync(ct);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedReason = "Logout";
            }

            db.RefreshTokens.UpdateRange(tokens);
            await db.SaveChangesAsync(ct);

            logger.LogInformation("User {UserId} logged out", authUser.UserId);

            return Success("Logout successful.", true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout");
            return Fail<bool>("An error occurred during logout.");
        }
    }

    // ── PRIVATE HELPERS ──────────────────────────────────────────────
    private string GenerateAccessToken(TUser user, List<string> roles, List<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Email, user.Email),
            new("Username", user.Username),
            new("OrganizationId", user.OrganizationId.ToString()),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(permissions.Select(perm => new Claim("Permission", perm)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _JwtSettings.Issuer,
            audience: _JwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(_JwtSettings.JwtExpires),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private ResponseModel<T> Fail<T>(string message)
        => new() { code = "99", success = false, message = message };

    private ResponseModel<T> Success<T>(string message, T data)
        => new() { code = "00", success = true, message = message, data = data };
}
