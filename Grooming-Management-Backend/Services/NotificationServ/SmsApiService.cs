using System.Text.Json;
using Grooming_Management_App.DTOs.SmsDTO;


namespace Grooming_Management_App.Services.NotificationServ;

public class SmsApiService(HttpClient httpClient, IConfiguration configuration, ILogger<SmsApiService> logger)
    : ISmsService
{
    private const string Endpoint = "https://api.smsapi.pl/sms.do";

    public async Task<SmsResponseDto> SendSmsAsync(string phoneNumber, string message, CancellationToken ct)
    {
        var token = configuration["SmsApi:Token"];
        var sender = configuration["SmsApi:Sender"];

        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogError("SmsApi:Token is not configured");
            return new SmsResponseDto { Success = false, ErrorMessage = "SMS provider not configured" };
        }

        var fields = new List<KeyValuePair<string, string>>
        {
            new("to", Normalize(phoneNumber)),
            new("message", message),
            new("format", "json"),
            new("encoding", "utf-8")
        };

        if (!string.IsNullOrWhiteSpace(sender))
        {
            fields.Add(new KeyValuePair<string, string>("from", sender));
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Content = new FormUrlEncodedContent(fields);

            using var response = await httpClient.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            using var json = JsonDocument.Parse(body);

            if (json.RootElement.TryGetProperty("error", out var error) && error.ValueKind != JsonValueKind.Null)
            {
                var description = json.RootElement.TryGetProperty("message", out var m)
                    ? m.GetString()
                    : "Unknown SMS provider error";

                logger.LogWarning("SMSAPI rejected message: {Error} {Description}", error, description);

                return new SmsResponseDto { Success = false, ErrorMessage = description };
            }

            return new SmsResponseDto { Success = true };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
            return new SmsResponseDto { Success = false, ErrorMessage = ex.Message };
        }
    }

    private static string Normalize(string phoneNumber)
    {
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

        return digits.Length == 9 ? "48" + digits : digits;
    }
}