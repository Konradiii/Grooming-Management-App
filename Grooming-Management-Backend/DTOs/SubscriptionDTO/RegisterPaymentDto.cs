namespace Grooming_Management_App.DTOs.SubscriptionDTO;

public class RegisterPaymentDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string ProviderId { get; set; }
    public string? InvoiceUrl { get; set; }
}