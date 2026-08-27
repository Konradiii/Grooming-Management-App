using Grooming_Management_App.Enums;

namespace Grooming_Management_App.DTOs.SubscriptionDTO;

public class GetSubscriptionDto
{
    public SubscriptionStatusEnum Status { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public bool HasActiveSubscription { get; set; }
    public List<GetPaymentDto> Payments { get; set; } = new();
}
