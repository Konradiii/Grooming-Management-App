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
public class DogController(IDogReaderService readerService,IDogWriterService writerService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Zwraca listę psów, opcjonalnie filtrowaną po właścicielu lub rasie")]
    public async Task<List<GetDogDto>> GetAllDogs(int? dogOwnerId, int? breedId, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var dogs = await readerService.GetAllDogsAsync(salonId, dogOwnerId, breedId, ct);
        return dogs;
    }

    [HttpGet("{dogId:int}")]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Zwraca szczegółowe dane pojedynczego psa")]
    public async Task<GetDogDetailsDto> GetDogDto(int dogId, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var dog = await readerService.GetDogDetailsAsync(salonId, dogId, ct);
        return dog;
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Dodaje nowego psa do kartoteki")]
    public async Task<IActionResult> CreateDog(CreateDogDto createDogDto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var newDogId = await writerService.CreateDogAsync(salonId, createDogDto, ct);
        return Created($"api/dog/{newDogId}", null);
    }

    [HttpPut("{dogId:int}")]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Edytuje dane istniejącego psa")]
    public async Task<IActionResult> UpdateDog(int dogId, UpdateDogDto updateDogDto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        await writerService.UpdateDogAsync(salonId, dogId, updateDogDto, ct);
        return NoContent();
    }
    
    [HttpPost("with-owner")]
    [Authorize(Roles = "Owner,Groomer")]
    [EndpointSummary("Tworzy jednocześnie nowego właściciela i jego psa")]
    public async Task<IActionResult> CreateDogWithOwner(CreateDogWithOwnerDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var newOwnerId = await writerService.CreateDogWithOwnerAsync(salonId, dto, ct);
        return Created($"api/DogOwner/{newOwnerId}", new { id = newOwnerId });
    }
    
}