using Microsoft.Extensions.Logging;
using TeamManage.Application.Interfaces.Services;

namespace TeamManage.Infrastructure.Email;

/// <summary>
/// Development stand-in for a real email provider: writes the message to the log.
/// Replace with an SMTP/SendGrid implementation of <see cref="IEmailSender"/> for production.
/// </summary>
public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        _logger.LogWarning("EMAIL to {To} | Subject: {Subject}\n{Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
