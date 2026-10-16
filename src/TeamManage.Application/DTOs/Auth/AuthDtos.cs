using TeamManage.Domain.Enums;

namespace TeamManage.Application.DTOs.Auth;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>
    /// Opaque, long-lived token the client can exchange for a new access token
    /// via /api/auth/refresh without re-prompting for a password. Store securely
    /// (e.g. platform secure storage) - never log or display it.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    public DateTime RefreshTokenExpiresAtUtc { get; set; }

    /// <summary>True when the user signed in via an invite link and must set a new password.</summary>
    public bool MustChangePassword { get; set; }
}

public class RedeemInviteRequest
{
    public string Token { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
