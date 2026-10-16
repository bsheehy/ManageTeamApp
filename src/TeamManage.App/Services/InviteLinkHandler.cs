using Microsoft.Extensions.DependencyInjection;

namespace TeamManage.App.Services;

/// <summary>
/// Handles teammanage://login?token=... deep links from invite emails: redeems the
/// one-time token for a session and, if required, sends the user to set a password.
/// A link received before the Shell is ready (cold start) is held until it loads.
/// </summary>
public static class InviteLinkHandler
{
    private static string? _pendingToken;

    public static bool HasPending => _pendingToken is not null;

    public static void Handle(Uri uri)
    {
        if (!string.Equals(uri.Scheme, "teammanage", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var token = System.Web.HttpUtility.ParseQueryString(uri.Query)["token"];
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        _pendingToken = token;

        // Warm start: the Shell already exists, so process immediately.
        if (Shell.Current is not null)
        {
            MainThread.BeginInvokeOnMainThread(async () => await ProcessPendingAsync());
        }
    }

    public static async Task ProcessPendingAsync()
    {
        var token = _pendingToken;
        _pendingToken = null;
        if (token is null || Shell.Current is null)
        {
            return;
        }

        try
        {
            var services = IPlatformApplication.Current!.Services;
            var authApi = services.GetRequiredService<IAuthApiService>();
            var session = services.GetRequiredService<ISessionService>();

            var auth = await authApi.RedeemInviteAsync(token);
            await session.SaveAsync(
                auth.Token,
                auth.UserId,
                auth.FullName,
                auth.Role,
                auth.ExpiresAtUtc,
                auth.RefreshToken,
                auth.RefreshTokenExpiresAtUtc);

            await Shell.Current.GoToAsync("//dashboard");
            if (auth.MustChangePassword)
            {
                await Shell.Current.GoToAsync("changepassword");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Invite link", ex.Message, "OK");
        }
    }
}
