using TeamManage.Application.Common;
using TeamManage.Application.DTOs.Auth;
using TeamManage.Application.Interfaces.Repositories;
using TeamManage.Application.Interfaces.Security;
using TeamManage.Application.Interfaces.Services;
using TeamManage.Domain.Entities;

namespace TeamManage.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthService(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthResponse>.Failure("Email and password are required.");
        }

        var user = await _users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            // Deliberately generic message so we don't reveal whether the email exists.
            return Result<AuthResponse>.Failure("Invalid email or password.");
        }

        return Result<AuthResponse>.Success(await IssueTokensAsync(user, ct));
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthResponse>.Failure("Full name, email and password are required.");
        }

        if (request.Password.Length < 6)
        {
            return Result<AuthResponse>.Failure("Password must be at least 6 characters long.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _users.ExistsByEmailAsync(email, ct))
        {
            return Result<AuthResponse>.Failure("An account with this email already exists.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role
        };

        await _users.AddAsync(user, ct);

        return Result<AuthResponse>.Success(await IssueTokensAsync(user, ct));
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result<AuthResponse>.Failure("Invalid or expired refresh token.");
        }

        var tokenHash = _tokenGenerator.HashRefreshToken(refreshToken);
        var existing = await _refreshTokens.GetByTokenHashAsync(tokenHash, ct);

        if (existing is null || !existing.IsActive || existing.User is null)
        {
            // Deliberately generic - don't reveal whether the token existed, expired, or was revoked.
            return Result<AuthResponse>.Failure("Invalid or expired refresh token.");
        }

        // Rotate: revoke the presented token so it can never be reused, even if intercepted.
        existing.RevokedAtUtc = DateTime.UtcNow;

        return Result<AuthResponse>.Success(await IssueTokensAsync(existing.User, ct));
    }

    /// <summary>
    /// Issues a fresh access + refresh token pair for a user, persists the new
    /// refresh token (hashed), and saves all pending changes in one transaction.
    /// </summary>
    private async Task<AuthResponse> IssueTokensAsync(User user, CancellationToken ct)
    {
        var (accessToken, accessExpiresAtUtc) = _tokenGenerator.GenerateToken(user);
        var (refreshToken, refreshExpiresAtUtc) = _tokenGenerator.GenerateRefreshToken();

        await _refreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenGenerator.HashRefreshToken(refreshToken),
            ExpiresAtUtc = refreshExpiresAtUtc
        }, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return new AuthResponse
        {
            Token = accessToken,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            ExpiresAtUtc = accessExpiresAtUtc,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAtUtc = refreshExpiresAtUtc
        };
    }
}
