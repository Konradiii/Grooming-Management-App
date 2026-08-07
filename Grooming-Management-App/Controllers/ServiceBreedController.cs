using Grooming_Management_App.DTOs.ServiceBreedDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.ServiceBreedServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("ServiceBreeds")]
public class ServiceBreedController(IServiceBreedService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPut("{serviceBreedId:int}/ActivateServiceBreed")]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Aktywuje wcześniej dezaktywowaną pozycję cennika")]
    public async Task<IActionResult> ActivateServiceBreed(int serviceBreedId, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        await service.ActivateServiceBreedAsync(salonId, serviceBreedId, ct);
        return Ok();
    }
    [HttpPut("{serviceBreedId:int}/DeactivateServiceBreed")]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Dezaktywuje pozycję cennika, bez usuwania historii")]
    public async Task<IActionResult> DeactivateServiceBreed(int serviceBreedId, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        await service.DeactivateServiceBreedAsync(salonId, serviceBreedId, ct);
        return Ok();
    }

    [HttpGet("GetAllServiceBreeds")]
    [Authorize]
    [EndpointSummary("Zwraca cennik, opcjonalnie filtrowany po statusie i rasie")]
    public async Task<List<GetServiceBreedDto>> GetAllServiceBreeds(ActiveStatusEnum? status, int? breedId, CancellationToken ct)
    {
        var salonId= currentUser.SalonId;
        var services = await service.GetAllServiceBreedsAsync(salonId, status, breedId, ct);
        return services;
    }

    [HttpGet("{serviceBreedId:int}")]
    [Authorize]
    [EndpointSummary("Zwraca pojedynczą pozycję cennika")]
    public async Task<GetServiceBreedDto> GetServiceBreed(int serviceBreedId, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var breedservice = await service.GetServiceBreedAsync(salonId, serviceBreedId, ct);
        return breedservice;
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Dodaje nową pozycję cennika - cenę i czas dla kombinacji usługa+rasa")]
    public async Task<IActionResult> AddService(CreateServiceBreedDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var newServiceBreedId = await service.AddServiceBreedAsync(salonId, dto, ct);
        return Created($"api/ServiceBreed/{newServiceBreedId}", null);
    }

    [HttpPut("{serviceBreedId:int}")]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Aktualizuje cenę i czas trwania istniejącej pozycji cennika")]
    public async Task<IActionResult> UpdateService(int serviceBreedId, UpdateServiceBreedDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        await service.UpdateServiceBreedAsync(salonId,serviceBreedId, dto, ct);
        return NoContent();
    }
}