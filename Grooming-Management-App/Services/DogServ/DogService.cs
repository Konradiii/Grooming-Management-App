using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.DogDTO;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.DogServ;

public class DogService(GroomingDbContext ctx) : IDogWriterService, IDogReaderService
{
    public async Task<List<GetDogDto>> GetAllDogsAsync(int salonId, int? dogOwnerId, int? breedId, CancellationToken ct)
    {

        return await ctx.Dogs
            .Where(e => e.SalonId == salonId)
            .Where(e => dogOwnerId == null || e.DogOwnerId == dogOwnerId)
            .Where(e=> breedId == null || e.BreedId == breedId)
            .Select(e => new GetDogDto
            {
                Id = e.Id,
                Name = e.Name,
                BreedName = e.Breed.Name,
                DogOwnerFullName = e.DogOwner.FirstName + " " + e.DogOwner.LastName,
                

            }).ToListAsync(ct);

    }

    public async Task<GetDogDetailsDto> GetDogDetailsAsync(int salonId, int dogId, CancellationToken ct)
    {
        var result = await ctx.Dogs
            .Where(e => e.SalonId == salonId && e.Id == dogId)
            .Select (e => new GetDogDetailsDto
            {
                Id = e.Id,
                Name = e.Name,
                AgeInMonths =  e.AgeInMonths,
                Notes = e.Notes,
                BreedName = e.Breed.Name,
                DogOwnerFullName = e.DogOwner.FirstName + " " + e.DogOwner.LastName,
                
            }
            ).FirstOrDefaultAsync(ct);

        if (result == null)
        {
            throw new NotFoundException("Dog not found");
        }
        return result;
    }

    public async Task<int> CreateDogAsync(int salonId, CreateDogDto dto, CancellationToken ct)
    {
        
        var dogOwnerExists = await ctx.DogOwners.AnyAsync(e => e.Id == dto.DogOwnerId && e.SalonId == salonId, ct);
        

        if (!dogOwnerExists)
        {
            throw new NotFoundException("DogOwner not found");
        }
        
        var newDog = new Dog
        {
            Name = dto.Name,
            AgeInMonths = dto.AgeInMonths,
            Notes = dto.Notes,
            BreedId = dto.BreedId,
            SalonId = salonId,
            DogOwnerId = dto.DogOwnerId,
        };
        
        
        await ctx.Dogs.AddAsync(newDog, ct);
        await ctx.SaveChangesAsync(ct);
        
        return newDog.Id;

    }

    public async Task UpdateDogAsync(int salonId, int dogId, UpdateDogDto dto, CancellationToken ct)
    {
        
        var dogExists = await ctx.Dogs.Where(e=> e.SalonId == salonId && e.Id== dogId).FirstOrDefaultAsync(ct);
        if (dogExists == null)
        {
            throw new NotFoundException("Dog not found");
        }
        
        var dogOwnerExists = await ctx.DogOwners
            .AnyAsync(d => d.Id == dto.DogOwnerId && d.SalonId == salonId, ct);
        
        if (!dogOwnerExists)
        {
            throw new NotFoundException("DogOwner not found");
        }
        
        dogExists.Name = dto.Name;
        dogExists.AgeInMonths = dto.AgeInMonths;
        dogExists.Notes = dto.Notes;
        dogExists.BreedId = dto.BreedId;
        dogExists.DogOwnerId = dto.DogOwnerId;
        
        await ctx.SaveChangesAsync(ct);



    }
}