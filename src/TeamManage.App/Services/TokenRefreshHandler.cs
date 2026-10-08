using System.Net;
using System.Net.Http.Json;
using TeamManage.Application.DTOs.Auth;

namespace TeamManage.App.Services;

/// <summary>
/// Outermost handler in the typed API clients' pipelines. When a request comes back
/// 401 Unauthorized, it attempts a one-time silent refresh of the access token using
/// the stored refresh token, then retries the original request. Concurrent 401s are
/// coalesced behind a single refresh call via a semaphore.
/// </summary>
public class TokenRefreshHandler : DelegatingHandler
{
    public const string RefreshClientName = "TokenRefreshClient";

    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    private readonly ISessionService _session;
    private readonly IHttpClientFactory _httpClientFactory;

    public TokenRefreshHandler(ISessionService session, IHttpClientFactory httpClientFactory)
    {
        _session = session;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        byte[]? bufferedContent = request.Content is null
            ? null
            : await request.Content.ReadAsByteArrayAsync(cancellationToken);

        var tokenUsedForRequest = _session.Token;

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized || !_session.IsAuthenticated)
        {
            return response;
        }

        var refreshed = await TryRefreshTokenAsync(tokenUsedForRequest, cancellationToken);
        if (!refreshed)
        {
            return response;
        }

        response.Dispose();
        var retryRequest = CloneRequest(request, bufferedContent);
        return await base.SendAsync(retryRequest, cancellationToken);
    }

    private async Task<bool> TryRefreshTokenAsync(string? tokenUsedForFailedRequest, CancellationToken cancellationToken)
    {
        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            // Another in-flight request may have already refreshed the session while we
            // were waiting for the lock - if so, just retry with the token that's now current.
            if (!string.Equals(_session.Token, tokenUsedForFailedRequest, StringComparison.Ordinal))
            {
                return true;
            }

            var refreshToken = _session.RefreshToken;
            if (string.IsNullOrEmpty(refreshToken))
            {
                return false;
            }

            var refreshClient = _httpClientFactory.CreateClient(RefreshClientName);
            var response = await refreshClient.PostAsJsonAsync(
                "api/auth/refresh",
                new RefreshTokenRequest { RefreshToken = refreshToken },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await _session.ClearAsync();
                return false;
            }

            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
            if (auth is null)
            {
                return false;
            }

            await _session.SaveAsync(
                auth.Token,
                auth.UserId,
                auth.FullName,
                auth.Role,
                auth.ExpiresAtUtc,
                auth.RefreshToken,
                auth.RefreshTokenExpiresAtUtc);

            return true;
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage original, byte[]? bufferedContent)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri)
        {
            Version = original.Version
        };

        foreach (var header in original.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (bufferedContent is not null)
        {
            clone.Content = new ByteArrayContent(bufferedContent);
            if (original.Content is not null)
            {
                foreach (var header in original.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        foreach (var option in original.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
        }

        return clone;
    }
}
