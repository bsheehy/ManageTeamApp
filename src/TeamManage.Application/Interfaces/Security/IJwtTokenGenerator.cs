using TeamManage.Domain.Entities;

namespace TeamManage.Application.Interfaces.Security;

/// <summary>
/// Issues signed JWT access tokens for authenticated users.
/// </summary>
public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);

    /// <summary>
    /// Generates a new cryptographically random opaque refresh token (not a JWT).
    /// The caller is responsible for hashing it before persisting - only the hash
    /// should ever be stored.
    /// </summary>
    (string Token, DateTime ExpiresAtUtc) GenerateRefreshToken();

    /// <summary>
    /// Hashes a raw refresh token for storage/lookup. Never persist or compare
    /// raw refresh token values directly.
    /// </summary>
    string HashRefreshToken(string token);
}
