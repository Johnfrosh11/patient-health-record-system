using Microsoft.Extensions.Logging;

namespace PatientHealthRecord.Infrastructure.Services.Email;

/// <summary>
/// Interface for email service - sends emails via SMTP or cloud provider
/// </summary>
public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken ct = default);
    Task<bool> SendBulkEmailAsync(List<string> recipients, string subject, string body, bool isHtml = true, CancellationToken ct = default);
}

/// <summary>
/// Email service implementation - configure with SMTP or SendGrid/AWS SES
/// TODO: Implement actual email sending logic
/// </summary>
public sealed class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken ct = default)
    {
        // TODO: Implement actual email sending
        _logger.LogInformation("Email would be sent to {To} with subject {Subject}", to, subject);
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> SendBulkEmailAsync(List<string> recipients, string subject, string body, bool isHtml = true, CancellationToken ct = default)
    {
        // TODO: Implement bulk email sending
        _logger.LogInformation("Bulk email would be sent to {Count} recipients", recipients.Count);
        await Task.CompletedTask;
        return true;
    }
}
