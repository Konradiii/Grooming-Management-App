using Grooming_Management_App.DTOs.SmsDTO;

namespace Grooming_Management_App.Services.NotificationServ;

public interface ISmsService
{
    Task<SmsResponseDto> SendSmsAsync(string phoneNumber, string message, CancellationToken ct);
}