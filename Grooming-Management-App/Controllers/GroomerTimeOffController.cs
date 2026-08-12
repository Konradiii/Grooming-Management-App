using Grooming_Management_App.DTOs.GroomerTimeOffDTO;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.GroomerTimeOffServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Owner")]
public class GroomerTimeOffController(IGroomerTimeOffService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Dodaje blokadę czasu dla pracownika - urlop lub wolne godziny")]
    public async Task<IActionResult> CreateGroomerTimeOff(CreateGroomerTimeOffDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var newTimeOffId = await service.CreateGroomerTimeOffAsync(salonId, dto, ct);
        return Created($"api/GroomerTimeOff/{newTimeOffId}", null);
    }

    [HttpGet("{timeOffId:int}")]
    [EndpointSummary("Zwraca szczegóły pojedynczej blokady czasu")]
    public async Task<GetGroomerTimeOffDto> GetGroomerTimeOff(int timeOffId, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        return await service.GetGroomerTimeOffAsync(salonId, timeOffId, ct);
    }

    [HttpGet]
    [EndpointSummary("Zwraca listę blokad czasu, opcjonalnie filtrowaną po pracowniku i zakresie dat")]
    public async Task<List<GetGroomerTimeOffDto>> GetAllGroomerTimeOffs(int? groomerId, DateOnly? dateFrom, DateOnly? dateTo, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        return await service.GetAllGroomerTimeOffsAsync(salonId, groomerId, dateFrom, dateTo, ct);
    }

    [HttpDelete("{timeOffId:int}")]
    [EndpointSummary("Usuwa blokadę czasu")]
    public async Task<IActionResult> DeleteGroomerTimeOff(int timeOffId, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        await service.DeleteGroomerTimeOffAsync(salonId, timeOffId, ct);
        return NoContent();
    }
}