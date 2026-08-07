using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.ServiceBreedDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.ServiceBreedServ;

public class ServiceBreedService(GroomingDbContext ctx) : IServiceBreedService
{
    public async Task ActivateServiceBreedAsync(int salonId, int serviceBreedId, CancellationToken ct)
    {
        var sbExists = await ctx.ServiceBreeds
            .Where(s => s.SalonId == salonId && s.Id == serviceBreedId)
            .FirstOrDefaultAsync(ct);

        if (sbExists == null)
        {
            throw new NotFoundException("Service on this breed not found");
        }
        if (sbExists.Status == ActiveStatusEnum.Active)
        {
            return;
        }

        sbExists.Status = ActiveStatusEnum.Active;
        await ctx.SaveChangesAsync(ct);
        
    }
    
    public async Task DeactivateServiceBreedAsync(int salonId, int serviceBreedId, CancellationToken ct)
    {
        var sbExists = await ctx.ServiceBreeds
            .Where(s => s.SalonId == salonId && s.Id == serviceBreedId)
            .FirstOrDefaultAsync(ct);

        if (sbExists == null)
        {
            throw new NotFoundException("Service on this breed not found");
        }

        if (sbExists.Status == ActiveStatusEnum.Inactive)
        {
            return;
        }

        sbExists.Status = ActiveStatusEnum.Inactive;
        await ctx.SaveChangesAsync(ct);
        
    }
    
    public async Task<List<GetServiceBreedDto>> GetAllServiceBreedsAsync(int salonId, ActiveStatusEnum? status, int? breedId, CancellationToken ct)
    {
        
        return await ctx.ServiceBreeds
            .Where(s => s.SalonId == salonId)
            .Where(s => status == null || s.Status == status)
            .Where(s=> breedId == null || s.BreedId == breedId)
            .Select(e=> new GetServiceBreedDto
            {
                Id = e.Id,
                Price = e.Price,
                Duration = e.Duration,
                ServiceName = e.Service.Name,
                BreedName = e.Breed.Name,
                Status = e.Status
                
            }).ToListAsync(ct);
        
    }
    
    public async Task<GetServiceBreedDto> GetServiceBreedAsync(int salonId, int serviceBreedId, CancellationToken ct)
    {
        var serviceBreed = await ctx.ServiceBreeds
            .Where(s => s.SalonId == salonId && s.Id == serviceBreedId)
            .Select(e=> new GetServiceBreedDto
            {
                Id = e.Id,
                Price = e.Price,
                Duration = e.Duration,
                ServiceName = e.Service.Name,
                BreedName = e.Breed.Name,
                Status = e.Status
            }).FirstOrDefaultAsync(ct);
        
        if (serviceBreed == null)
        {
            throw new NotFoundException("Service on this breed not found");
            
        }
        return serviceBreed;
        
    }

    public async Task<int> AddServiceBreedAsync(int salonId, CreateServiceBreedDto dto, CancellationToken ct)
    {
        var serviceExists = await ctx.Services
            .Where(e => dto.ServiceId == e.Id && salonId == e.SalonId)
            .AnyAsync(ct);

        if (!serviceExists)
        {
            throw new NotFoundException("Service doesnt exists");
        }
        
        var breedExists = await ctx.Breeds
            .Where(e => e.Id == dto.BreedId)
            .AnyAsync(ct);
        
        if (!breedExists)
        {
            throw new NotFoundException("Breed doesnt exists");
        }
        
        var combinationExists = await ctx.ServiceBreeds
            .AnyAsync(sb => sb.ServiceId == dto.ServiceId 
                            && sb.BreedId == dto.BreedId 
                            && sb.SalonId == salonId, ct);

        if (combinationExists)
        {
            throw new ConflictException("This service/breed combination already exists in the pricing");
        }
        
        var newServiceBreed = new ServiceBreed
        {
            Price = dto.Price,
            Duration = dto.Duration,
            Status = ActiveStatusEnum.Active,
            SalonId = salonId,
            ServiceId = dto.ServiceId,
            BreedId = dto.BreedId
        };
        await ctx.ServiceBreeds.AddAsync(newServiceBreed);
        await ctx.SaveChangesAsync(ct);
        return newServiceBreed.Id;

    }
    
    public async Task UpdateServiceBreedAsync(int salonId, int serviceBreedId, UpdateServiceBreedDto dto, CancellationToken ct)
    {
        
        var serviceBreed = await ctx.ServiceBreeds
            .Where(s => s.SalonId == salonId && s.Id == serviceBreedId)
            .FirstOrDefaultAsync(ct);
        if (serviceBreed == null)
        {
            throw new NotFoundException("Service on this breed not found");
        }
        serviceBreed.Price = dto.Price;
        serviceBreed.Duration = dto.Duration;
        
        await ctx.SaveChangesAsync(ct);
    }
}