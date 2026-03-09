using Microsoft.Extensions.Logging;

namespace PatientHealthRecord.Infrastructure.Services.Sms;

/// <summary>
/// Interface for SMS service - sends SMS via Twilio, AWS SNS, etc.
/// </summary>
public interface ISmsService
{
    Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken ct = default);
    Task<bool> SendBulkSmsAsync(List<string> phoneNumbers, string message, CancellationToken ct = default);
}

/// <summary>
/// SMS service implementation - configure with Twilio or AWS SNS
/// TODO: Implement actual SMS sending logic
/// </summary>
public sealed class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        // TODO: Implement actual SMS sending
        _logger.LogInformation("SMS would be sent to {PhoneNumber}", phoneNumber);
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> SendBulkSmsAsync(List<string> phoneNumbers, string message, CancellationToken ct = default)
    {
        // TODO: Implement bulk SMS sending
        _logger.LogInformation("Bulk SMS would be sent to {Count} phone numbers", phoneNumbers.Count);
        await Task.CompletedTask;
        return true;
    }
}
