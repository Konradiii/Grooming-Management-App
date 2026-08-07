using Grooming_Management_App.DTOs.GroomerDTO;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.GroomerServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroomerController(IGroomerService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPut("{id:int}/DeactivateGroomer")]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Dezaktywuje pracownika, bez usuwania historii wizyt")]
    public async Task<IActionResult> DeactivateGroomer(int id, CancellationToken ct)
    {
        var salonId =  currentUser.SalonId;
        await service.DeactivateGroomerAsync(id, salonId, ct);
        return NoContent();
    }
    [HttpPut("{id:int}/ActivateGroomer")]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Ponownie aktywuje wcześniej dezaktywowanego pracownika")]
    public async Task<IActionResult> ActivateGroomer(int id, CancellationToken ct)
    {
        var salonId =  currentUser.SalonId;
        await service.ActivateGroomerAsync(id, salonId, ct);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Zwraca dane pojedynczego pracownika")]
    public async Task<GetGroomerDto> GetGroomer(int id, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var groomer = await service.GetGroomerAsync(id, salonId, ct);
        return groomer;
    }

    [HttpGet]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Zwraca listę wszystkich pracowników salonu")]
    public async Task<List<GetGroomerDto>> GetAllGroomers(CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var groomers = await service.GetAllGroomersAsync(salonId, ct);
        return groomers;
    }

    [HttpPut("{id:int}/EditGroomer")]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Edytuje dane pracownika")]
    public async Task<IActionResult> EditGroomer(int id, [FromBody] EditGroomerDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        await service.EditGroomerAsync(dto, id, salonId, ct);
        return NoContent();
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Dodaje nowego pracownika, bez konta logowania")]
    public async Task<IActionResult> CreateGroomer([FromBody] CreateGroomerDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var newGroomerId = await service.CreateGroomerAsync(dto, salonId, ct);
        return Created($"api/Groomer/{newGroomerId}", null);
    }
    
}