using Grooming_Management_App.DTOs.SubscriptionDTO;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Services.SubscriptionServ;
using Stripe;
using Stripe.Checkout;

namespace Grooming_Management_App.Services.StripeServ;

public class StripeService(
    IConfiguration configuration,
    ISubscriptionService subscriptionService,
    ILogger<StripeService> logger) : IStripeService
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
    
    

    public async Task HandleWebhookAsync(string json, string signature, CancellationToken ct)
    {
        var webhookSecret = configuration["Stripe:WebhookSecret"]
                            ?? throw new InvalidOperationException("Stripe WebhookSecret is not configured");

        Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Invalid Stripe webhook signature");
            throw new UnauthorizedException(ErrorCodes.InvalidCredentials);
        }

        logger.LogInformation("Stripe webhook received: {EventType} ({EventId})",
            stripeEvent.Type, stripeEvent.Id);

        switch (stripeEvent.Type)
        {
            case "checkout.session.completed":
                await HandleCheckoutCompletedAsync(stripeEvent, ct);
                break;

            case "invoice.paid":
                await HandleInvoicePaidAsync(stripeEvent, ct);
                break;

            case "invoice.payment_failed":
                await HandleInvoiceFailedAsync(stripeEvent, ct);
                break;
            
            case "customer.subscription.deleted":
                await HandleSubscriptionDeletedAsync(stripeEvent, ct);
                break;
            
            case "customer.subscription.updated":
                await HandleSubscriptionUpdatedAsync(stripeEvent, ct);
                break;
            

            default:
                logger.LogInformation("Ignoring Stripe event type {EventType}", stripeEvent.Type);
                break;
        }
    }
    private async Task HandleInvoicePaidAsync(Event stripeEvent, CancellationToken ct)
    {
        if (stripeEvent.Data.Object is not Invoice invoice)
        {
            logger.LogWarning("invoice.paid without invoice object");
            return;
        }
        
        if (invoice.Parent?.SubscriptionDetails?.SubscriptionId == null)
        {
            logger.LogInformation("Invoice {InvoiceId} is not subscription-related, ignoring", invoice.Id);
            return;
        }

        var salonId = await subscriptionService.GetSalonIdByCustomerIdAsync(invoice.CustomerId, ct);

        if (salonId == null)
        {
            salonId = await ResolveSalonIdFromSubscriptionAsync(invoice, ct);
        }

        if (salonId == null)
        {
            logger.LogWarning("invoice.paid for unknown customer {CustomerId}", invoice.CustomerId);
            return;
        }

        var dto = new RegisterPaymentDto
        {
            Amount = invoice.AmountPaid / 100m,
            Currency = invoice.Currency.ToUpperInvariant(),
            ProviderId = invoice.Id,
            InvoiceUrl = invoice.HostedInvoiceUrl
        };

        try
        {
            var validUntil = await subscriptionService.RegisterPaymentAsync(salonId.Value, dto, ct);

            logger.LogInformation("Payment registered for salon {SalonId}, valid until {ValidUntil}",
                salonId, validUntil);
        }
        catch (ConflictException)
        {
            logger.LogInformation("Invoice {InvoiceId} already processed, ignoring", invoice.Id);
        }
    }

       private async Task HandleCheckoutCompletedAsync(Event stripeEvent, CancellationToken ct)
    {
        if (stripeEvent.Data.Object is not Session session)
        {
            logger.LogWarning("checkout.session.completed without session object");
            return;
        }

        if (!TryGetSalonId(session.Metadata, session.ClientReferenceId, out var salonId))
        {
            logger.LogWarning("checkout.session.completed without salonId, session {SessionId}", session.Id);
            return;
        }

        if (session.Metadata != null
            && session.Metadata.TryGetValue("type", out var type)
            && type == "sms_topup")
        {
            await HandleSmsTopUpAsync(session, salonId, ct);
            return;
        }

        await subscriptionService.LinkProviderIdsAsync(
            salonId, session.CustomerId, session.SubscriptionId, ct);

        logger.LogInformation("Salon {SalonId} linked to Stripe customer {CustomerId}",
            salonId, session.CustomerId);
    }

    private async Task HandleSmsTopUpAsync(Session session, int salonId, CancellationToken ct)
    {
        if (session.Metadata == null
            || !session.Metadata.TryGetValue("smsCount", out var raw)
            || !int.TryParse(raw, out var smsCount))
        {
            logger.LogWarning("SMS top-up session {SessionId} without smsCount", session.Id);
            return;
        }

        var dto = new RegisterPaymentDto
        {
            Amount = (session.AmountTotal ?? 0) / 100m,
            Currency = session.Currency?.ToUpperInvariant() ?? "PLN",
            ProviderId = session.Id,
            InvoiceUrl = null
        };

        try
        {
            await subscriptionService.AddPurchasedSmsAsync(salonId, smsCount, dto, ct);

            logger.LogInformation("Added {SmsCount} SMS to salon {SalonId}", smsCount, salonId);
        }
        catch (ConflictException)
        {
            logger.LogInformation("SMS top-up session {SessionId} already processed, ignoring", session.Id);
        }
    }

    private async Task HandleInvoiceFailedAsync(Event stripeEvent, CancellationToken ct)
    {
        if (stripeEvent.Data.Object is not Invoice invoice)
        {
            logger.LogWarning("invoice.payment_failed without invoice object");
            return;
        }
        
        if (invoice.Parent?.SubscriptionDetails?.SubscriptionId == null)
        {
            logger.LogInformation("Invoice {InvoiceId} is not subscription-related, ignoring", invoice.Id);
            return;
        }

        var salonId = await subscriptionService.GetSalonIdByCustomerIdAsync(invoice.CustomerId, ct);

        if (salonId == null)
        {
            logger.LogWarning("invoice.payment_failed for unknown customer {CustomerId}", invoice.CustomerId);
            return;
        }

        var dto = new RegisterPaymentDto
        {
            Amount = invoice.AmountDue / 100m,
            Currency = invoice.Currency.ToUpperInvariant(),
            ProviderId = invoice.Id,
            InvoiceUrl = invoice.HostedInvoiceUrl
        };

        try
        {
            await subscriptionService.MarkPaymentFailedAsync(salonId.Value, dto, ct);
            logger.LogWarning("Payment failed for salon {SalonId}, invoice {InvoiceId}", salonId, invoice.Id);
        }
        catch (ConflictException)
        {
            logger.LogInformation("Failed invoice {InvoiceId} already processed, ignoring", invoice.Id);
        }
    }

    private static bool TryGetSalonId(
        IDictionary<string, string>? metadata, string? clientReferenceId, out int salonId)
    {
        salonId = 0;

        if (metadata != null
            && metadata.TryGetValue("salonId", out var fromMetadata)
            && int.TryParse(fromMetadata, out salonId))
        {
            return true;
        }

        return int.TryParse(clientReferenceId, out salonId);
    }
    
    private async Task<int?> ResolveSalonIdFromSubscriptionAsync(Invoice invoice, CancellationToken ct)
    {
        var subscriptionId = invoice.Parent?.SubscriptionDetails?.SubscriptionId;

        if (string.IsNullOrEmpty(subscriptionId))
            return null;

        var subService = new Stripe.SubscriptionService();
        var subscription = await subService.GetAsync(subscriptionId, cancellationToken: ct);

        if (subscription.Metadata == null
            || !subscription.Metadata.TryGetValue("salonId", out var raw)
            || !int.TryParse(raw, out var salonId))
        {
            return null;
        }

        await subscriptionService.LinkProviderIdsAsync(salonId, invoice.CustomerId, subscriptionId, ct);

        return salonId;
    }
    
    public async Task<string> CreatePortalSessionAsync(string customerId, CancellationToken ct)
    {
        var returnUrl = configuration["Stripe:SuccessUrl"]
                        ?? throw new InvalidOperationException("Stripe SuccessUrl is not configured");

        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = customerId,
            ReturnUrl = returnUrl
        };

        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(options, cancellationToken: ct);

        return session.Url;
    }
    
    private async Task HandleSubscriptionDeletedAsync(Event stripeEvent, CancellationToken ct)
    {
        if (stripeEvent.Data.Object is not Subscription subscription)
        {
            logger.LogWarning("customer.subscription.deleted without subscription object");
            return;
        }

        await subscriptionService.ClearSubscriptionAsync(subscription.CustomerId, ct);

        logger.LogInformation("Subscription {SubscriptionId} cancelled for customer {CustomerId}",
            subscription.Id, subscription.CustomerId);
    }
    
    private async Task HandleSubscriptionUpdatedAsync(Event stripeEvent, CancellationToken ct)
    {
        if (stripeEvent.Data.Object is not Stripe.Subscription subscription)
            return;

        await subscriptionService.SetCancelAtPeriodEndAsync(
            subscription.CustomerId, subscription.CancelAtPeriodEnd, ct);

        logger.LogInformation("Subscription {SubscriptionId} cancel_at_period_end = {Value}",
            subscription.Id, subscription.CancelAtPeriodEnd);
    }
    
    public async Task<string> CreateSmsTopUpSessionAsync(int salonId, string email, int packageSize, CancellationToken ct)
    {
        var priceId = configuration[$"Stripe:SmsPackages:{packageSize}"];

        if (string.IsNullOrWhiteSpace(priceId))
        {
            throw new ConflictException(ErrorCodes.InvalidSmsPackage);
        }

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            LineItems = new List<SessionLineItemOptions>
            {
                new() { Price = priceId, Quantity = 1 }
            },
            CustomerEmail = email,
            ClientReferenceId = salonId.ToString(),
            Metadata = new Dictionary<string, string>
            {
                ["salonId"] = salonId.ToString(),
                ["type"] = "sms_topup",
                ["smsCount"] = packageSize.ToString()
            },
            SuccessUrl = configuration["Stripe:SuccessUrl"],
            CancelUrl = configuration["Stripe:CancelUrl"]
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: ct);

        return session.Url;
    }
    
    
}