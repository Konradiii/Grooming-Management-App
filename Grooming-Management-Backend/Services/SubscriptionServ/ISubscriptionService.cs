using Grooming_Management_App.DTOs.SubscriptionDTO;

namespace Grooming_Management_App.Services.SubscriptionServ;

public interface ISubscriptionService
{
    Task<DateOnly> RegisterPaymentAsync(int salonId, RegisterPaymentDto dto, CancellationToken ct);
    
    Task MarkPaymentFailedAsync(int salonId, RegisterPaymentDto dto, CancellationToken ct);
    
    Task<int> SuspendExpiredSubscriptionsAsync(CancellationToken ct);
    
    Task<int> MarkExpiredSubscriptionsAsPastDueAsync(CancellationToken ct);
    
    Task LinkProviderIdsAsync(int salonId, string? customerId, string? subscriptionId, CancellationToken ct);
    
    Task<int?> GetSalonIdByCustomerIdAsync(string? customerId, CancellationToken ct);
    
    Task<GetSubscriptionDto> GetSubscriptionAsync(int salonId, CancellationToken ct);
    
    Task<string?> GetProviderCustomerIdAsync(int salonId, CancellationToken ct);
}