using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Services.StripeServ;

public interface IStripeService
{
    Task<string> CreateCheckoutSessionAsync(int salonId, string salonName, string email, PlanTypeEnum plan, bool withTrial, CancellationToken ct);
    Task HandleWebhookAsync(string json, string signature, CancellationToken ct);

    Task<string> CreatePortalSessionAsync(string customerId, CancellationToken ct);

    Task<string> CreateSmsTopUpSessionAsync(int salonId, string email, int packageSize, CancellationToken ct);
    
    

    
}