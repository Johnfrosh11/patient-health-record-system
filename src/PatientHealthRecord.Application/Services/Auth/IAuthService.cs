using PatientHealthRecord.Application.DTOs.Auth;
using PatientHealthRecord.Utilities;

namespace PatientHealthRecord.Application.Services.Auth;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    Task<ResponseModel<AuthViewModel>> LoginAsync(LoginDto model, CancellationToken ct = default);
    Task<ResponseModel<AuthViewModel>> RegisterAsync(RegisterDto model, CancellationToken ct = default);
    Task<ResponseModel<AuthViewModel>> RefreshTokenAsync(RefreshTokenDto model, CancellationToken ct = default);
    Task<ResponseModel<bool>> LogoutAsync(CancellationToken ct = default);
}
