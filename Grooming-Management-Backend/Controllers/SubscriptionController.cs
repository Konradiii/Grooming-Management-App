using Grooming_Management_App.DTOs.SubscriptionDTO;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.SubscriptionServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Owner")]
public class SubscriptionController(ISubscriptionService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPost("payment")]
    [EndpointSummary("Rejestruje płatność i przedłuża subskrypcję o miesiąc (tymczasowe - docelowo webhook)")]
    public async Task<IActionResult> RegisterPayment(RegisterPaymentDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var validUntil = await service.RegisterPaymentAsync(salonId, dto, ct);
        return Ok(new { validUntil });
    }
}