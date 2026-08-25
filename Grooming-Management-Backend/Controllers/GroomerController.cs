using Grooming_Management_App.DTOs.GroomerDTO;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.GroomerServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroomerController(IGroomerReaderService readerService, IGroomerWriterService writerService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPut("{id:int}/DeactivateGroomer")]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Dezaktywuje pracownika, bez usuwania historii wizyt")]
    public async Task<IActionResult> DeactivateGroomer(int id, CancellationToken ct)
    {
        var salonId =  currentUser.SalonId;
        await writerService.DeactivateGroomerAsync(id, salonId, ct);
        return NoContent();
    }
    [HttpPut("{id:int}/ActivateGroomer")]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Ponownie aktywuje wcześniej dezaktywowanego pracownika")]
    public async Task<IActionResult> ActivateGroomer(int id, CancellationToken ct)
    {
        var salonId =  currentUser.SalonId;
        await writerService.ActivateGroomerAsync(id, salonId, ct);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Zwraca dane pojedynczego pracownika")]
    public async Task<GetGroomerDto> GetGroomer(int id, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var groomer = await readerService.GetGroomerAsync(id, salonId, ct);
        return groomer;
    }

    [HttpGet]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Zwraca listę wszystkich pracowników salonu")]
    public async Task<List<GetGroomerDto>> GetAllGroomers(CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var groomers = await readerService.GetAllGroomersAsync(salonId, ct);
        return groomers;
    }

    [HttpPut("{id:int}/EditGroomer")]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Edytuje dane pracownika")]
    public async Task<IActionResult> EditGroomer(int id, [FromBody] EditGroomerDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        await writerService.EditGroomerAsync(dto, id, salonId, ct);
        return NoContent();
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Dodaje nowego pracownika, bez konta logowania")]
    public async Task<IActionResult> CreateGroomer([FromBody] CreateGroomerDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var newGroomerId = await writerService.CreateGroomerAsync(dto, salonId, ct);
        return Created($"api/Groomer/{newGroomerId}", null);
    }

    [HttpGet("basic")]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Zwraca uproszczoną listę groomerów salonu — bez danych rozliczeniowych")]
    public async Task<ActionResult<List<GetGroomerBasicDto>>> GetAllBasic(CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var groomers = await readerService.GetAllGroomersBasicAsync(salonId, ct);
        return groomers;
    }
    
    [HttpGet("me")]
    [Authorize(Roles = "Owner,Groomer")]
    public async Task<ActionResult<GetGroomerBasicDto?>> GetMe(CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var groomer = await readerService.GetCurrentGroomerAsync(salonId, ct);
        return Ok(groomer);
    }
}