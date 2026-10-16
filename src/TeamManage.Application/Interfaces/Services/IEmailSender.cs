namespace TeamManage.Application.Interfaces.Services;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}

/// <summary>
/// Deep link used in invite emails. Opens the mobile app via its custom URL scheme.
/// </summary>
public static class InviteLink
{
    public const string Scheme = "teammanage";
}
