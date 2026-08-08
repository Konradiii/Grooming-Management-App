using System.Xml;
using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.VisitDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.VisitServ;

public class VisitService(GroomingDbContext ctx) : IVisitService
{
    public async Task<List<GetAllVisitsDto>> GetAllVisitsAsync(int salonId, VisitFilterDto filter, CancellationToken ct)
    {
        return await ctx.Visits
            .Where(v => v.SalonId == salonId)
            .Where(v => filter.GroomerId == null || filter.GroomerId == v.GroomerId)
            .Where(e => filter.DateFrom == null || e.Date > filter.DateFrom )
            .Where(e => filter.DateTo == null ||  e.Date <= filter.DateTo)
            .Select(v => new GetAllVisitsDto
            {
                Id = v.Id,
                Date = v.Date,
                DogName = v.Dog.Name,
                GroomerName = v.Groomer.FirstName + " " + v.Groomer.LastName,
                ServiceName = v.ServiceBreed.Service.Name,
                Status = v.Status

            }).ToListAsync(ct);
    }
    
    public async Task<GetVisitDetailsDto> GetVisitAsync(int salonId, int visitId, CancellationToken ct)
    {

        var visit = await ctx.Visits
            .Where(v => v.SalonId == salonId)
            .Where(v => v.Id == visitId)
            .Select(v => new GetVisitDetailsDto
            {
                Id = v.Id,
                CreatedAt =  v.CreatedAt,
                Date =  v.Date,
                EstimatedDuration =  v.EstimatedDuration,
                ProposedPrice =   v.ProposedPrice,
                FinalPrice =  v.FinalPrice,
                Status =  v.Status,
                Notes = v.Notes,
                DogName = v.Dog.Name,
                DogOwnerFullName =  v.DogOwner.FirstName + " " + v.DogOwner.LastName,
                GroomerFullName =  v.Groomer.FirstName + " " + v.Groomer.LastName,
                ServiceName = v.ServiceBreed.Service.Name,
                BreedName =  v.ServiceBreed.Breed.Name
                
            }).FirstOrDefaultAsync(ct);

        if (visit == null)
        {
            throw new NotFoundException("Visit not found");
        }
        
        return visit;

    }
    
    public async Task<int> AddVisitAsync(int salonId, AddVisitDto dto, CancellationToken ct)
    {
        

        var serviceBreed = await ctx.ServiceBreeds
            .Where(g => salonId == g.SalonId)
            .Where(d => d.Id == dto.ServiceBreedId)
            .FirstOrDefaultAsync(ct);
        if (serviceBreed == null)
        {
            throw new NotFoundException("Service breed not found");
        }
        var dog = await ctx.Dogs
            .Where(g => salonId == g.SalonId)
            .Where(d => d.Id == dto.DogId)
            .FirstOrDefaultAsync(ct);
        if (dog == null)
        {
            throw new NotFoundException("Dog not found");
        }
        var groomerExists = await ctx.Groomers
            .Where(g => salonId == g.SalonId)
            .Where(d => d.Id == dto.GroomerId)
            .AnyAsync(ct);
        if (!groomerExists)
        {
            throw new NotFoundException("Groomer not found");
        }

        if (serviceBreed.BreedId != dog.BreedId)
        {
            throw new ConflictException("Service Breed doeasnt exists for that breed");
        }
        
        var duplicateExists = await ctx.Visits
            .AnyAsync(v => v.DogId == dto.DogId 
                           && v.Date == dto.Date 
                           && v.SalonId == salonId, ct);

        if (duplicateExists)
        {
            throw new ConflictException("This dog already has a visit scheduled at this exact time");
        }

        var newVisit = new Visit
        {
            CreatedAt = DateTime.UtcNow,
            Date = dto.Date,
            EstimatedDuration = serviceBreed.Duration,
            ProposedPrice = serviceBreed.Price,
            Status = StatusEnum.Scheduled,
            Notes = dto.Notes,
            SalonId = salonId,
            DogId = dto.DogId,
            DogOwnerId = dog.DogOwnerId,
            GroomerId = dto.GroomerId,
            ServiceBreedId = dto.ServiceBreedId
        };
        
        ctx.Visits.Add(newVisit);
        await ctx.SaveChangesAsync(ct);
        return newVisit.Id;
        
        
    }
    
    
    public async Task EditVisitAsync(int salonId, int visitId, EditVisitDto dto, CancellationToken ct)
    {
        
        var visit = await ctx.Visits
            .Where(v => v.SalonId == salonId &&  v.Id == visitId)
            .FirstOrDefaultAsync(ct);

        if (visit == null)
        {
            throw new NotFoundException("Visit not found");
        }
        var groomerExist = await ctx.Groomers.Where(g => salonId == g.SalonId && g.Id == dto.GroomerId).AnyAsync(ct);
        if (!groomerExist)
        {
            throw new NotFoundException("Groomer not found");
        }
        visit.Date = dto.Date;
        visit.GroomerId = dto.GroomerId;
        visit.Notes = dto.Notes;
        await ctx.SaveChangesAsync(ct);
        
    }
    
  
    
    public async Task ChangeVisitStatusAsync(int salonId, int visitId, StatusEnum status, CancellationToken ct)
    {
        var visit =  await ctx.Visits
            .Where(v => v.SalonId == salonId && v.Id == visitId).FirstOrDefaultAsync(ct);
        if (visit == null)
        {
            throw new NotFoundException("Visit not found");
        }
        visit.Status = status;
        await ctx.SaveChangesAsync(ct);
        
    }
    
    public async Task UpdateFinalPriceAsync(int salonId, int visitId, decimal finalPrice, CancellationToken ct)
    {
        var visit =  await ctx.Visits
            .Where(v => v.SalonId == salonId && v.Id == visitId).FirstOrDefaultAsync(ct);
        if (visit == null)
        {
            throw new NotFoundException("Visit not found");
        }
        visit.FinalPrice = finalPrice;
        await ctx.SaveChangesAsync(ct);
        
        
    }

}