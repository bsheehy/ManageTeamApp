using TeamManage.Application.Common;
using TeamManage.Application.DTOs.Auth;

namespace TeamManage.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);

    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    /// <summary>
    /// Exchanges a valid, unexpired refresh token for a new access token,
    /// rotating (revoking and replacing) the refresh token in the process.
    /// </summary>
    Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}
