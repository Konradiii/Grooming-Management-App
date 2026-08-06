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
    [Authorize]
    public async Task<List<GetAllVisitsDto>> GetAllVisits(VisitFilterDto filter, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        var visits = await service.GetAllVisitsAsync(salonId, filter, ct);
        return visits;
    }

    [HttpGet("{visitId:int}")]
    [Authorize]
    public async Task<GetVisitDetailsDto> GetVisit(int visitId, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        var visit = await service.GetVisitAsync(salonId, visitId, ct);
        return visit;
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddVisit(AddVisitDto dto, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        await service.AddVisitAsync(salonId, dto, ct);
        return Created($"api/visit/{salonId}", null);
    }
    [HttpPut("{visitId:int}")]
    [Authorize]
    public async Task<IActionResult> EditVisit(int visitId, EditVisitDto dto, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        await service.EditVisitAsync(salonId, visitId, dto, ct);
        return NoContent();
    }
    [HttpPut("{visitId:int}/status")]
    [Authorize]
    public async Task<IActionResult> ChangeVisitStatus(int visitId, StatusEnum status, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        await service.ChangeVisitStatusAsync(salonId, visitId, status, ct);
        return NoContent();
    }

    [HttpPut("{visitId:int}/final-price")]
    [Authorize]
    public async Task<IActionResult> UpdateFinalPrice(int visitId, decimal finalPrice, CancellationToken ct)
    {
        var salonId = userService.SalonId;
        await service.UpdateFinalPriceAsync(salonId, visitId, finalPrice, ct);
        return NoContent();
    }
    
}