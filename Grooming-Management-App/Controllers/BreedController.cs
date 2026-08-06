using Grooming_Management_App.DTOs.Breed;
using Grooming_Management_App.Services.Breed;
using Grooming_Management_App.Services.CurrentUserServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BreedController(IBreedService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<List<GetBreedDto>> GetAllBreeds(CancellationToken ct)
    {
        return await service.GetAllBreedsAsync(ct);
    }

    [HttpGet("{Id:int}")]
    [Authorize]
    public async Task<GetBreedDto> GetBreedAsync(int Id, CancellationToken ct)
    {
        return await service.GetBreedAsync(Id, ct);
    }
    
}