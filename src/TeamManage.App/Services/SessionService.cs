using System.Globalization;
using TeamManage.Domain.Enums;

namespace TeamManage.App.Services;

/// <summary>
/// Holds the current signed-in user's session for the lifetime of the app,
/// and persists/restores it from secure device storage between launches.
/// </summary>
public interface ISessionService
{
    string? Token { get; }

    string? RefreshToken { get; }

    DateTime AccessTokenExpiresAtUtc { get; }

    DateTime RefreshTokenExpiresAtUtc { get; }

    Guid UserId { get; }

    string FullName { get; }

    UserRole Role { get; }

    bool IsAuthenticated { get; }

    Task SaveAsync(
        string token,
        Guid userId,
        string fullName,
        UserRole role,
        DateTime accessTokenExpiresAtUtc,
        string refreshToken,
        DateTime refreshTokenExpiresAtUtc);

    Task RestoreAsync();

    Task ClearAsync();
}

public class SessionService : ISessionService
{
    private const string TokenKey = "auth_token";
    private const string UserIdKey = "auth_user_id";
    private const string FullNameKey = "auth_full_name";
    private const string RoleKey = "auth_role";
    private const string RefreshTokenKey = "auth_refresh_token";
    private const string AccessTokenExpiresKey = "auth_access_expires";
    private const string RefreshTokenExpiresKey = "auth_refresh_expires";

    public string? Token { get; private set; }

    public string? RefreshToken { get; private set; }

    public DateTime AccessTokenExpiresAtUtc { get; private set; }

    public DateTime RefreshTokenExpiresAtUtc { get; private set; }

    public Guid UserId { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public async Task SaveAsync(
        string token,
        Guid userId,
        string fullName,
        UserRole role,
        DateTime accessTokenExpiresAtUtc,
        string refreshToken,
        DateTime refreshTokenExpiresAtUtc)
    {
        Token = token;
        UserId = userId;
        FullName = fullName;
        Role = role;
        AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc;
        RefreshToken = refreshToken;
        RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc;

        await SecureStorage.Default.SetAsync(TokenKey, token);
        await SecureStorage.Default.SetAsync(UserIdKey, userId.ToString());
        await SecureStorage.Default.SetAsync(FullNameKey, fullName);
        await SecureStorage.Default.SetAsync(RoleKey, role.ToString());
        await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);
        await SecureStorage.Default.SetAsync(AccessTokenExpiresKey, accessTokenExpiresAtUtc.ToString("o", CultureInfo.InvariantCulture));
        await SecureStorage.Default.SetAsync(RefreshTokenExpiresKey, refreshTokenExpiresAtUtc.ToString("o", CultureInfo.InvariantCulture));
    }

    public async Task RestoreAsync()
    {
        Token = await SecureStorage.Default.GetAsync(TokenKey);
        var userIdText = await SecureStorage.Default.GetAsync(UserIdKey);
        var roleText = await SecureStorage.Default.GetAsync(RoleKey);
        var accessExpiresText = await SecureStorage.Default.GetAsync(AccessTokenExpiresKey);
        var refreshExpiresText = await SecureStorage.Default.GetAsync(RefreshTokenExpiresKey);

        UserId = Guid.TryParse(userIdText, out var id) ? id : Guid.Empty;
        FullName = await SecureStorage.Default.GetAsync(FullNameKey) ?? string.Empty;
        Role = Enum.TryParse<UserRole>(roleText, out var role) ? role : UserRole.Player;
        RefreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
        AccessTokenExpiresAtUtc = DateTime.TryParse(accessExpiresText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var accessExpires)
            ? accessExpires
            : DateTime.MinValue;
        RefreshTokenExpiresAtUtc = DateTime.TryParse(refreshExpiresText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var refreshExpires)
            ? refreshExpires
            : DateTime.MinValue;
    }

    public Task ClearAsync()
    {
        Token = null;
        RefreshToken = null;
        AccessTokenExpiresAtUtc = DateTime.MinValue;
        RefreshTokenExpiresAtUtc = DateTime.MinValue;
        UserId = Guid.Empty;
        FullName = string.Empty;
        SecureStorage.Default.RemoveAll();
        return Task.CompletedTask;
    }
}
