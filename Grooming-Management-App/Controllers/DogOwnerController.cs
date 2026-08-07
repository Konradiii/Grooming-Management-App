using Grooming_Management_App.DTOs.DogOwner;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.DogOwner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DogOwnerController(IDogOwnerService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Zwraca dane pojedynczego właściciela psa")]
    public async Task<GetDogOwnerDto> GetDogOwner(int id, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var owner = await service.GetDogOwnerAsync(id, salonId, ct);
        return owner;
    }

    [HttpGet]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Zwraca listę wszystkich właścicieli psów w salonie")]
    public async Task<List<GetDogOwnerDto>> GetAllDogOwners(CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var owners = await service.GetAllDogOwnersAsync(salonId, ct);
        return owners;
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Tworzy nowego właściciela psa")]
    public async Task<IActionResult> CreateDogOwner(CreateDogOwnerDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var ownerId =await service.CreateDogOwnerAsync(dto, salonId, ct);
        return Created($"api/DogOwner/{ownerId}", null);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Edytuje dane istniejącego właściciela psa")]
    public async Task<IActionResult> EditDogOwner(EditDogOwnerDto dto, int id, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        await service.EditDogOwnerAsync(dto, id, salonId, ct);
        return NoContent();
    }
}