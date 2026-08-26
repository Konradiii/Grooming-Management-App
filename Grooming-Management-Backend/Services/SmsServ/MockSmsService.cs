using Grooming_Management_App.DTOs.SmsDTO;

namespace Grooming_Management_App.Services.NotificationServ;

public class MockSmsService(ILogger<MockSmsService> logger) : ISmsService
{
    public Task<SmsResponseDto> SendSmsAsync(string phoneNumber, string message, CancellationToken ct)
    {
        logger.LogInformation("[MOCK SMS] To: {PhoneNumber}, Message: {Message}", phoneNumber, message);
        return Task.FromResult(new SmsResponseDto { Success = true });
    }
}