using Grooming_Management_App.DTOs.SmsDTO;

namespace Grooming_Management_App.Services.NotificationServ;

public class MockSmsService : ISmsService
{
    public Task<SmsResponseDto> SendSmsAsync(string phoneNumber, string message, CancellationToken ct)
    {
        Console.WriteLine($"[MOCK SMS] To: {phoneNumber}, Message: {message}");
        return Task.FromResult(new SmsResponseDto { Success = true });
    }
}