using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamManage.Application.DTOs.Auth;
using TeamManage.Application.Interfaces.Services;

namespace TeamManage.Api.Controllers;

[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticates a user and issues a JWT access token.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        if (!result.Succeeded)
        {
            // 401 (not 400) for auth failures - avoids leaking whether validation
            // or credentials were the cause, per OWASP authentication guidance.
            return Unauthorized(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Registers a new user account (Manager or Player) and issues a JWT access token.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);
        if (!result.Succeeded)
        {
            return ProblemFrom(result.Error!);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Exchanges a valid refresh token for a new access token, so a mobile client
    /// can silently renew a session after its short-lived access token expires
    /// (e.g. after being offline) without the user re-entering their password.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, ct);
        if (!result.Succeeded)
        {
            return Unauthorized(new { error = result.Error });
        }

        return Ok(result.Data);
    }
}
