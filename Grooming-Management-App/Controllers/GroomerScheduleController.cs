using Grooming_Management_App.DTOs.GroomerScheduleDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.GroomerScheduleServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroomerScheduleController(IGroomerScheduleService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles= "Owner")]    
    public async Task<IActionResult> CreateGroomerSchedule(CreateGroomerScheduleDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        
        var newScheduleId = await service.CreateGroomerScheduleAsync(salonId, dto, ct);
        
        return Created($"api/GroomerSchedule/{newScheduleId}", null);
        


    }
    [HttpGet("{groomerScheduleId:int}")]
    [Authorize(Roles= "Owner")]    
    public async Task<GetGroomerScheduleDto> GetGroomerSchedule(int groomerScheduleId, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        
        var groomerSchedule = await service.GetGroomerScheduleAsync(salonId, groomerScheduleId, ct);
        
        return groomerSchedule;
        
        
    }
    [HttpGet]
    [Authorize(Roles= "Owner")]    
    public async Task<List<GetGroomerScheduleDto>> GetAllGroomerSchedule(int? groomerId, DayOfWeekEnum? day, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        
        return await  service.GetAllGroomerScheduleAsync(salonId, groomerId, day, ct);
        
    }
    [HttpDelete("{groomerScheduleId:int}")]
    [Authorize(Roles= "Owner")]    
    public async Task<IActionResult> DeleteGroomerSchedule(int groomerScheduleId, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        
        await service.DeleteGroomerScheduleAsync(salonId, groomerScheduleId, ct);

        return NoContent();

    }
    
}