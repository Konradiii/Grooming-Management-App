using Grooming_Management_App.DTOs.WaitlistDTO;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.WaitlistServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WaitlistController(IWaitlistService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Owner,Groomer")]    
    public async Task<IActionResult> AddToWaitlistAsync(CreateWaitlistDto dto, CancellationToken ct){
        
        var salonId = currentUser.SalonId;
        
        var resultId = await service.AddToWaitlistAsync(salonId, dto, ct);
        
        return Created($"api/Waitlist/{resultId}", null);
    }
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Owner,Groomer")]    
    public async Task<IActionResult> RemoveFromWaitlistAsync(int id, CancellationToken ct){
        
        var salonId = currentUser.SalonId;
        
        await service.RemoveFromWaitlistAsync(salonId, id, ct);
        return NoContent();

        
    }
    [HttpGet]
    [Authorize(Roles = "Owner,Groomer")]    
    public async Task<List<GetWaitlistDto>> GetAllWaitlistAsync(CancellationToken ct){
        
        var salonId = currentUser.SalonId;
        return await service.GetAllWaitlistAsync(salonId, ct);

        
    }

}