using Grooming_Management_App.DTOs.VisitDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.VisitServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Visits")]
public class VisitController(IVisitReaderService readerService,IVisitWriterService writerService, ICurrentUserService userService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Zwraca listę wizyt, z filtrami po statusie, groomerze i zakresie dat")]
    public async Task<List<GetAllVisitsDto>> GetAllVisits([FromQuery] VisitFilterDto filter, CancellationToken ct)
    {
        filter.DateFrom = filter.DateFrom.AsUtc();
        filter.DateTo = filter.DateTo.AsUtc();

        var salonId = userService.SalonId;
        return await readerService.GetAllVisitsAsync(salonId, filter, ct);
    }

    [HttpGet("{visitId:int}")]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Zwraca szczegóły pojedynczej wizyty")]
    public async Task<GetVisitDetailsDto> GetVisit(int visitId, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        var visit = await readerService.GetVisitAsync(salonId, visitId, ct);
        return visit;
    }
    [HttpPost]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Edytuje termin, groomera lub notatki wizyty")]
    public async Task<IActionResult> AddVisit(AddVisitDto dto, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        var visitId = await writerService.AddVisitAsync(salonId, dto, ct);
        return Created($"api/visit/{visitId}", null);
    }
    [HttpPut("{visitId:int}")]
    [Authorize]
    [EndpointSummary("Tworzy nową wizytę - cena i czas pobierane automatycznie z cennika")]
    public async Task<IActionResult> EditVisit(int visitId, EditVisitDto dto, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        await writerService.EditVisitAsync(salonId, visitId, dto, ct);
        return NoContent();
    }
    [HttpPut("{visitId:int}/status")]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Zmienia status wizyty, np. na Ukończona lub Anulowana")]
    public async Task<IActionResult> ChangeVisitStatus(int visitId, StatusEnum status, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        await writerService.ChangeVisitStatusAsync(salonId, visitId, status, ct);
        return NoContent();
    }

    [HttpPut("{visitId:int}/final-price")]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Nadpisuje cenę finalną wizyty")]
    public async Task<IActionResult> UpdateFinalPrice(int visitId, decimal finalPrice, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        await writerService.UpdateFinalPriceAsync(salonId, visitId, finalPrice, ct);
        return NoContent();
    }
    
    [HttpPost("with-new-dog")]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Tworzy wizytę wraz z nowym klientem i psem")]
    public async Task<IActionResult> CreateVisitWithNewDog(CreateVisitWithNewDogDto dto, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        var newVisitId = await writerService.CreateVisitWithNewDogAsync(salonId, dto, ct);
        return Created($"api/Visit/{newVisitId}", new { id = newVisitId });
    }
    
    /*
    [HttpPost("book")]
    [Authorize(Roles = "Client")]
    [EndpointSummary("Rezerwacja wizyty przez zalogowanego klienta")]
    public async Task<IActionResult> BookVisitByClient(AddVisitDto dto, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        var userId = userService.UserId;
        var newVisitId = await writerService.BookVisitByClientAsync(salonId, userId, dto, ct);
        return Created($"api/Visit/{newVisitId}", null);
    }
    */
}