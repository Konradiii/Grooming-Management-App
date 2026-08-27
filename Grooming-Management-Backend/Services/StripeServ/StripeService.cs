using Stripe;
using Stripe.Checkout;

namespace Grooming_Management_App.Services.StripeServ;

public class StripeService(IConfiguration configuration) : IStripeService
{
    public async Task<string> CreateCheckoutSessionAsync(int salonId, string salonName, string email, CancellationToken ct)
    {
        var priceId = configuration["Stripe:PriceId"]
                      ?? throw new InvalidOperationException("Stripe PriceId is not configured");

        var successUrl = configuration["Stripe:SuccessUrl"]
                         ?? throw new InvalidOperationException("Stripe SuccessUrl is not configured");

        var cancelUrl = configuration["Stripe:CancelUrl"]
                        ?? throw new InvalidOperationException("Stripe CancelUrl is not configured");

        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            CustomerEmail = string.IsNullOrWhiteSpace(email) ? null : email,
            LineItems = new List<SessionLineItemOptions>
            {
                new() { Price = priceId, Quantity = 1 }
            },
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            ClientReferenceId = salonId.ToString(),
            Metadata = new Dictionary<string, string>
            {
                ["salonId"] = salonId.ToString(),
                ["salonName"] = salonName
            },
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["salonId"] = salonId.ToString()
                }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: ct);

        return session.Url;
    }
}