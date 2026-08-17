using Grooming_Management_App.DTOs.VisitDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.VisitServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Visits")]
public class VisitController(IVisitService service, ICurrentUserService userService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Zwraca listę wizyt, z filtrami po statusie, groomerze i zakresie dat")]
    public async Task<List<GetAllVisitsDto>> GetAllVisits([FromQuery] VisitFilterDto filter, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        var visits = await service.GetAllVisitsAsync(salonId, filter, ct);
        return visits;
    }

    [HttpGet("{visitId:int}")]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Zwraca szczegóły pojedynczej wizyty")]
    public async Task<GetVisitDetailsDto> GetVisit(int visitId, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        var visit = await service.GetVisitAsync(salonId, visitId, ct);
        return visit;
    }
    [HttpPost]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Edytuje termin, groomera lub notatki wizyty")]
    public async Task<IActionResult> AddVisit(AddVisitDto dto, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        var visitId = await service.AddVisitAsync(salonId, dto, ct);
        return Created($"api/visit/{visitId}", null);
    }
    [HttpPut("{visitId:int}")]
    [Authorize]
    [EndpointSummary("Tworzy nową wizytę - cena i czas pobierane automatycznie z cennika")]
    public async Task<IActionResult> EditVisit(int visitId, EditVisitDto dto, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        await service.EditVisitAsync(salonId, visitId, dto, ct);
        return NoContent();
    }
    [HttpPut("{visitId:int}/status")]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Zmienia status wizyty, np. na Ukończona lub Anulowana")]
    public async Task<IActionResult> ChangeVisitStatus(int visitId, StatusEnum status, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        await service.ChangeVisitStatusAsync(salonId, visitId, status, ct);
        return NoContent();
    }

    [HttpPut("{visitId:int}/final-price")]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Nadpisuje cenę finalną wizyty")]
    public async Task<IActionResult> UpdateFinalPrice(int visitId, decimal finalPrice, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        await service.UpdateFinalPriceAsync(salonId, visitId, finalPrice, ct);
        return NoContent();
    }
    
    /*
    [HttpPost("book")]
    [Authorize(Roles = "Client")]
    [EndpointSummary("Rezerwacja wizyty przez zalogowanego klienta")]
    public async Task<IActionResult> BookVisitByClient(AddVisitDto dto, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        var userId = userService.UserId;
        var newVisitId = await service.BookVisitByClientAsync(salonId, userId, dto, ct);
        return Created($"api/Visit/{newVisitId}", null);
    }
    */
}