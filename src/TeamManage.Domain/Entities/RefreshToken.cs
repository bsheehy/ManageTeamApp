namespace TeamManage.Domain.Entities;

/// <summary>
/// An opaque, long-lived token that lets a client silently obtain a new JWT
/// access token without the user re-entering their password - important for
/// a mobile app where the user may reopen the app after the short-lived
/// access token has expired while offline. Only a hash of the token is
/// stored; the raw value is returned to the client once and never persisted.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public User? User { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Set when the token has been rotated (replaced by a newer refresh token)
    /// or explicitly revoked (e.g. on logout). A revoked token can never be used again.
    /// </summary>
    public DateTime? RevokedAtUtc { get; set; }

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
}
