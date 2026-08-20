using Grooming_Management_App.DTOs.SalonDTO;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.SalonServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalonController(ISalonService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Zwraca dane własnego salonu")]
    public async Task<GetSalonDto> GetSalon(CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        return await service.GetSalonAsync(salonId, ct);
    }

    [HttpPut]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Zmienia nazwę salonu")]
    public async Task<IActionResult> UpdateSalon(UpdateSalonDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        await service.UpdateSalonAsync(dto, salonId, ct);
        return NoContent();
    }
    
}