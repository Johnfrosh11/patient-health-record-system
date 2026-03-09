namespace PatientHealthRecord.Application.DTOs.Auth;

// ============================================================================
// LOGIN
// ============================================================================
public class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Type alias for backward compatibility
public sealed class LoginRequest : LoginDto { }

// ============================================================================
// REGISTER
// ============================================================================
public class RegisterDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
}

// Type alias for backward compatibility
public sealed class RegisterRequest : RegisterDto
{
    public string ConfirmPassword { get; set; } = string.Empty;
}

// ============================================================================
// REFRESH TOKEN
// ============================================================================
public class RefreshTokenDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

// Type alias for backward compatibility
public sealed class RefreshTokenRequest : RefreshTokenDto { }

// ============================================================================
// AUTH VIEW MODEL (Response)
// ============================================================================
public class AuthViewModel
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}

// Type alias for backward compatibility
public sealed class AuthResponse : AuthViewModel { }

// ============================================================================
// LOGOUT
// ============================================================================
public sealed class LogoutDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
