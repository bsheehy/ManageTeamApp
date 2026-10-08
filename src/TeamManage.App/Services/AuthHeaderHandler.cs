using System.Net.Http.Headers;

namespace TeamManage.App.Services;

/// <summary>
/// Attaches the current session's JWT bearer token to every outgoing API request.
/// </summary>
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ISessionService _session;

    public AuthHeaderHandler(ISessionService session)
    {
        _session = session;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_session.IsAuthenticated)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.Token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
