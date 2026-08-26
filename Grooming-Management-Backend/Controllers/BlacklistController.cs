using Grooming_Management_App.DTOs.BlacklistDto;
using Grooming_Management_App.DTOs.VisitDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Services.BlacklistServ;
using Grooming_Management_App.Services.CurrentUserServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Owner")]
public class BlacklistController(IBlacklistService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Owner,Groomer")]
    public async Task<List<GetAllBlacklistDto>> GetAllClientsOfBlacklist(string? phoneNumber, CancellationToken ct)
    {
        
        var salonId = currentUser.SalonId;

        return await service.GetAllClientsOfBlacklistAsync(salonId, phoneNumber, ct);

    }
    
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Owner,Groomer")]
    public async Task<GetDetailsBlackListDto> GetDetailsBlackList(int id, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        
        return await service.GetDetailsBlackListAsync(salonId, id, ct);

        
    }
    [HttpPost("ByDogOwner")]
    [Authorize(Roles = "Owner,Groomer")]
    public async Task<IActionResult> AddToBlacklistByDogOwner([FromBody] CreateBlacklistByDogOwnerDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;

        var newRecordId = await service.AddToBlacklistByDogOwnerAsync(salonId, dto, ct);
        
        return Created($"api/Blacklist/{newRecordId}", null);


    }
    [HttpPost("ByDog")]
    [Authorize(Roles = "Owner,Groomer")]
    public async Task<IActionResult> AddToBlacklistByDog(CreateBlacklistByDogDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        
        var newRecordId = await service.AddToBlacklistByDogAsync(salonId, dto, ct);

        return Created($"api/Blacklist/{newRecordId}", null);


    }
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Owner,Groomer")]
    public async Task<IActionResult> DeleteRecordFromBlacklist(int id, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        
        await service.DeleteRecordFromBlacklistAsync(salonId, id, ct);

        return NoContent();


    }

    
}