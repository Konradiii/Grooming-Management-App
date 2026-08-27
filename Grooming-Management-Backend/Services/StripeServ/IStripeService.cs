namespace Grooming_Management_App.Services.StripeServ;

public interface IStripeService
{
    Task<string> CreateCheckoutSessionAsync(int salonId, string salonName, string email, CancellationToken ct);
}