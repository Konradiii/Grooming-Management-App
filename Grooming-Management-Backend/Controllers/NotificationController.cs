using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.NotificationServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Owner,Groomer")]
public class NotificationController(INotificationService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPost("ready-for-pickup")]
    public async Task<IActionResult> SendReadyForPickupNotification(int visitId, int timeToPickUpDogInMin, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        await service.SendReadyForPickupNotificationAsync(salonId, visitId, timeToPickUpDogInMin, ct);
        return Ok();
    }
}