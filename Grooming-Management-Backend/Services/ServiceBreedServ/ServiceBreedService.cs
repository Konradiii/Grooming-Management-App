using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.ServiceBreedDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.ServiceBreedServ;

public class ServiceBreedService(GroomingDbContext ctx) : IServiceBreedWriterService, IServiceBreedReaderService
{
    public async Task ActivateServiceBreedAsync(int salonId, int serviceBreedId, CancellationToken ct)
    {
        var sbExists = await ctx.ServiceBreeds
            .Where(s => s.SalonId == salonId && s.Id == serviceBreedId)
            .FirstOrDefaultAsync(ct);

        if (sbExists == null)
        {
            throw new NotFoundException(ErrorCodes.ServiceBreedNotFound);
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
            throw new NotFoundException(ErrorCodes.ServiceBreedNotFound);
        }

        if (sbExists.Status == ActiveStatusEnum.Inactive)
        {
            return;
        }

        sbExists.Status = ActiveStatusEnum.Inactive;
        await ctx.SaveChangesAsync(ct);

    }

    public async Task<List<GetServiceBreedDto>> GetAllServiceBreedsAsync(int salonId, ActiveStatusEnum? status,
        int? breedId, CancellationToken ct)
    {

        return await ctx.ServiceBreeds
            .Where(s => s.SalonId == salonId)
            .Where(s => status == null || s.Status == status)
            .Where(s => breedId == null || s.BreedId == breedId)
            .Select(e => new GetServiceBreedDto
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
            .Select(e => new GetServiceBreedDto
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
            throw new NotFoundException(ErrorCodes.ServiceBreedNotFound);

        }

        return serviceBreed;

    }

    public async Task<int> AddServiceBreedAsync(int salonId, CreateServiceBreedDto dto, CancellationToken ct)
    {
        if (dto.Price <= 0)
            throw new ConflictException(ErrorCodes.InvalidPrice);

        if (dto.Duration <= 0)
            throw new ConflictException(ErrorCodes.InvalidDuration);

        var serviceExists = await ctx.Services
            .Where(e => dto.ServiceId == e.Id && salonId == e.SalonId)
            .AnyAsync(ct);

        if (!serviceExists)
        {
            throw new NotFoundException(ErrorCodes.ServiceNotFound);
        }

        var breedExists = await ctx.Breeds
            .Where(e => e.Id == dto.BreedId)
            .AnyAsync(ct);

        if (!breedExists)
        {
            throw new NotFoundException(ErrorCodes.BreedNotFound);
        }

        var combinationExists = await ctx.ServiceBreeds
            .AnyAsync(sb => sb.ServiceId == dto.ServiceId
                            && sb.BreedId == dto.BreedId
                            && sb.SalonId == salonId, ct);

        if (combinationExists)
        {
            throw new ConflictException(ErrorCodes.ServiceBreedCombinationExists);
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

        ctx.ServiceBreeds.Add(newServiceBreed);
        await ctx.SaveChangesAsync(ct);
        return newServiceBreed.Id;
    }

    public async Task UpdateServiceBreedAsync(int salonId, int serviceBreedId, UpdateServiceBreedDto dto,
        CancellationToken ct)
    {
        if (dto.Price <= 0)
            throw new ConflictException(ErrorCodes.InvalidPrice);

        if (dto.Duration <= 0)
            throw new ConflictException(ErrorCodes.InvalidDuration);

        var serviceBreed = await ctx.ServiceBreeds
            .Where(s => s.SalonId == salonId && s.Id == serviceBreedId)
            .FirstOrDefaultAsync(ct);

        if (serviceBreed == null)
        {
            throw new NotFoundException(ErrorCodes.ServiceBreedNotFound);
        }

        serviceBreed.Price = dto.Price;
        serviceBreed.Duration = dto.Duration;

        await ctx.SaveChangesAsync(ct);
    }

    public async Task<int> CreateServiceBreedWithServiceAsync(int salonId, CreateServiceBreedWithServiceDto dto,
        CancellationToken ct)
    {
        Validate.NotEmpty(dto.ServiceName, ErrorCodes.NameRequired);

        if (dto.Price <= 0)
            throw new ConflictException(ErrorCodes.InvalidPrice);

        if (dto.Duration <= 0)
            throw new ConflictException(ErrorCodes.InvalidDuration);

        var trimmedName = dto.ServiceName.Trim();

        var serviceExists = await ctx.Services
            .AnyAsync(s => s.Name == trimmedName && s.SalonId == salonId, ct);

        if (serviceExists)
        {
            throw new ConflictException(ErrorCodes.ServiceNameTaken);
        }

        var breedExists = await ctx.Breeds.AnyAsync(b => b.Id == dto.BreedId, ct);

        if (!breedExists)
        {
            throw new NotFoundException(ErrorCodes.BreedNotFound);
        }

        var service = new Service
        {
            Name = trimmedName,
            Status = ActiveStatusEnum.Active,
            SalonId = salonId
        };

        var serviceBreed = new ServiceBreed
        {
            BreedId = dto.BreedId,
            Price = dto.Price,
            Duration = dto.Duration,
            Status = ActiveStatusEnum.Active,
            SalonId = salonId
        };

        service.ServiceBreeds.Add(serviceBreed);

        ctx.Services.Add(service);
        await ctx.SaveChangesAsync(ct);

        return serviceBreed.Id;
    }
}