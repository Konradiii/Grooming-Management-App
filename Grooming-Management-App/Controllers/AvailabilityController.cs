using Grooming_Management_App.DTOs.AvailabilityDTO;
using Grooming_Management_App.Services.AvailabilityServ;
using Grooming_Management_App.Services.CurrentUserServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Owner,Groomer")]
public class AvailabilityController(IAvailabilityService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Zwraca wolne terminy na dany dzień dla wybranej usługi, opcjonalnie dla konkretnego pracownika")]
    public async Task<List<GetAvailabilityDto>> GetAvailabilitySlots(DateOnly date, int serviceBreedId, int? groomerId, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        return await service.GetAvailabilitySlotsAsync(salonId, date, serviceBreedId, groomerId, ct);
    }
}