using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.ServiceDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.ServiceServ;

public class ServiceService(GroomingDbContext ctx) : IServiceReaderService, IServiceWriterService
{
    public async Task<List<GetServiceDto>> GetAllServicesAsync(int salonId, ActiveStatusEnum? status, CancellationToken ct)
    {
        return await ctx.Services
            .Where(s => s.SalonId == salonId)
            .Where(s => status == null || s.Status == status)
            .Select(e => new GetServiceDto
            {
                Id = e.Id,
                Name = e.Name,
                Status = e.Status
                
            }).ToListAsync(ct);
    }
    
    public async Task<GetServiceDto> GetServiceAsync(int salonId, int serviceId, CancellationToken ct)
    {

        var service = await ctx.Services
            .Where(s => s.SalonId == salonId && s.Id == serviceId)
            .Select(e=> new GetServiceDto
            {
                Id = e.Id,
                Name = e.Name,
                Status = e.Status
            })
            .FirstOrDefaultAsync(ct);

        if (service == null)
        {
            throw new NotFoundException(ErrorCodes.ServiceNotFound);
        }

        return service;

    }
    
    public async Task ActivateServiceAsync(int salonId, int serviceId ,CancellationToken ct)
    {
        
        var service = await  ctx.Services
            .Where(s => s.SalonId == salonId && s.Id == serviceId)
            .FirstOrDefaultAsync(ct);

        if (service == null)
        {
            throw new NotFoundException(ErrorCodes.ServiceNotFound);
        }

        if (service.Status == ActiveStatusEnum.Active)
        {
            return;
        }
        
        service.Status = ActiveStatusEnum.Active;
        await ctx.SaveChangesAsync(ct);
        
        
        
    }
    
    public async Task DeactivateServiceAsync(int salonId, int serviceId, CancellationToken ct)
    {
        var service = await  ctx.Services
            .Where(s => s.SalonId == salonId && s.Id == serviceId)
            .FirstOrDefaultAsync(ct);

        if (service == null)
        {
            throw new NotFoundException(ErrorCodes.ServiceNotFound);
        }

        if (service.Status == ActiveStatusEnum.Inactive)
        {
            return;
        }
        
        service.Status = ActiveStatusEnum.Inactive;
        await ctx.SaveChangesAsync(ct);


        
    }
    public async Task<int> AddServiceAsync(int salonId, string newName, CancellationToken ct)
    {
        Validate.NotEmpty(newName, ErrorCodes.NameRequired);

        var trimmedName = newName.Trim();

        var serviceExist = await ctx.Services
            .AnyAsync(s => s.Name == trimmedName && s.SalonId == salonId, ct);

        if (serviceExist)
        {
            throw new ConflictException(ErrorCodes.ServiceNameTaken);
        }

        var newService = new Service
        {
            Name = trimmedName,
            Status = ActiveStatusEnum.Active,
            SalonId = salonId
        };
        ctx.Services.Add(newService);
        await ctx.SaveChangesAsync(ct);
        return newService.Id;
    }

    public async Task EditNameServiceAsync(int salonId, int serviceId, string newName, CancellationToken ct)
    {
        Validate.NotEmpty(newName, ErrorCodes.NameRequired);

        var trimmedName = newName.Trim();

        var service = await ctx.Services
            .Where(s => s.SalonId == salonId && s.Id == serviceId)
            .FirstOrDefaultAsync(ct);

        if (service == null)
        {
            throw new NotFoundException(ErrorCodes.ServiceNotFound);
        }

        var nameTaken = await ctx.Services
            .AnyAsync(s => s.Name == trimmedName
                           && s.SalonId == salonId
                           && s.Id != serviceId, ct);

        if (nameTaken)
        {
            throw new ConflictException(ErrorCodes.ServiceNameTaken);
        }

        service.Name = trimmedName;
        await ctx.SaveChangesAsync(ct);
    }
    
}