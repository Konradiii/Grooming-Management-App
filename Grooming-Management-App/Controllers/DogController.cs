using Grooming_Management_App.DTOs.Breed;
using Grooming_Management_App.DTOs.DogDTO;
using Grooming_Management_App.Models;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.DogServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DogController(IDogService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Owner,Groomer")]
    public async Task<List<GetDogDto>> GetAllDogs(int? dogOwnerId, int? breedId, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var dogs = await service.GetAllDogsAsync(salonId, dogOwnerId, breedId, ct);
        return dogs;
    }

    [HttpGet("{dogId:int}")]
    [Authorize(Roles = "Owner,Groomer")]
    public async Task<GetDogDetailsDto> GetDogDto(int dogId, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var dog = await service.GetDogDetailsAsync(salonId, dogId, ct);
        return dog;
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Groomer")]
    public async Task<IActionResult> CreateDog(CreateDogDto createDogDto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var newDogId = await service.CreateDogAsync(salonId, createDogDto, ct);
        return Created($"api/dog/{newDogId}", null);
    }

    [HttpPut("{dogId:int}")]
    [Authorize(Roles = "Owner,Groomer")]
    public async Task<IActionResult> UpdateDog(int dogId, UpdateDogDto updateDogDto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        await service.UpdateDogAsync(salonId, dogId, updateDogDto, ct);
        return NoContent();
    }
    
}