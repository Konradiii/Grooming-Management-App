using Grooming_Management_App.DTOs.SubscriptionDTO;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.SalonServ;
using Grooming_Management_App.Services.StripeServ;
using Grooming_Management_App.Services.SubscriptionServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Owner")]
public class SubscriptionController(ISubscriptionService service, ICurrentUserService currentUser, ISalonService salonService, IStripeService stripeService) : ControllerBase
{
    [HttpPost("payment")]
    [EndpointSummary("Rejestruje płatność i przedłuża subskrypcję o miesiąc (tymczasowe - docelowo webhook)")]
    public async Task<IActionResult> RegisterPayment(RegisterPaymentDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var validUntil = await service.RegisterPaymentAsync(salonId, dto, ct);
        return Ok(new { validUntil });
    }
    
    [HttpPost("checkout")]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Tworzy sesję płatności Stripe i zwraca adres do przekierowania")]
    public async Task<ActionResult<string>> CreateCheckout(CancellationToken ct)
    {
        
        var claims = string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"));
        Console.WriteLine(claims);
        
        
        var salonId = currentUser.SalonId;
        var salon = await salonService.GetSalonAsync(salonId, ct);

        var url = await stripeService.CreateCheckoutSessionAsync(
            salonId, salon.Name, currentUser.Email, ct);

        return Ok(url);
    }
}