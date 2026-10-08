namespace TeamManage.Infrastructure.Security;

/// <summary>
/// JWT signing configuration, bound from the "Jwt" configuration section.
/// <see cref="Secret"/> must be supplied via configuration/user-secrets/environment
/// variables in every environment - never committed to source control.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 480;

    /// <summary>
    /// How long a refresh token remains valid. Kept long (default 30 days) relative
    /// to the access token so a mobile user who goes offline, or simply doesn't
    /// open the app for a while, doesn't need to re-enter their password as long
    /// as they reconnect within this window.
    /// </summary>
    public int RefreshTokenExpiryDays { get; set; } = 30;
}
