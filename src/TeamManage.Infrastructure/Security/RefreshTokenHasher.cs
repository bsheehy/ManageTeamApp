using System.Security.Cryptography;
using System.Text;

namespace TeamManage.Infrastructure.Security;

/// <summary>
/// Hashes refresh tokens before they're persisted, so a stolen database backup
/// alone cannot be replayed as a valid refresh token. A plain fast hash (rather
/// than PBKDF2) is appropriate here: unlike a user-chosen password, a refresh
/// token is a 64-byte cryptographically random value, immune to dictionary/
/// brute-force attacks regardless of hash speed.
/// </summary>
public static class RefreshTokenHasher
{
    public static string Hash(string token) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
