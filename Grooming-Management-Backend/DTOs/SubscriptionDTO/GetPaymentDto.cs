using Grooming_Management_App.Enums;


public class GetPaymentDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public PaymentStatusEnum Status { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public string? InvoiceUrl { get; set; }
}