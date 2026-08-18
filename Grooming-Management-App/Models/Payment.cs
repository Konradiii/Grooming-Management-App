using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Models;

public class Payment
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency {get; set;}
    public DateTime PaymentDate { get; set; }
    public string ProviderId { get; set; }
    public PaymentStatusEnum Status { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public string? InvoiceUrl { get; set; }
    
    
    public int SalonId { get; set; }
    public Salon Salon { get; set; }
}