using Grooming_Management_App.DTOs.Breed;
using Grooming_Management_App.Services.Breed;
using Grooming_Management_App.Services.CurrentUserServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BreedController(IBreedReaderService readerService, IBreedWriterService writerService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [Authorize]
    [EndpointSummary("Zwraca listę wszystkich dostępnych ras")]
    public async Task<List<GetBreedDto>> GetAllBreeds(CancellationToken ct)
    {
        var salonId = currentUser.SalonId;

        return await readerService.GetAllBreedsAsync(salonId, ct);
    }

    [HttpGet("{Id:int}")]
    [Authorize]
    [EndpointSummary("Zwraca szczegóły pojedynczej rasy")]
    public async Task<GetBreedDto> GetBreedAsync(int Id, CancellationToken ct)
    {
        return await readerService.GetBreedAsync(Id, ct);
    }
    
    [HttpPost]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Dodaje rasę widoczną tylko w tym salonie")]
    public async Task<IActionResult> CreateBreed(CreateBreedDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var newBreedId = await writerService.CreateBreedAsync(salonId, dto, ct);

        return Created($"api/Breed/{newBreedId}", null);
    }
    
}