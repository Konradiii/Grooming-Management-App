using System.Xml;
using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.VisitDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Grooming_Management_App.Services.AvailabilityServ;
using Grooming_Management_App.Services.BlacklistServ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.VisitServ;

public class VisitService(GroomingDbContext ctx, IBlacklistCheckService blacklistCheckService, IAvailabilityReaderService availabilityReaderService) : IVisitService
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

        var isBlocked = await blacklistCheckService.IsBlockedAsync(salonId, dog.DogOwnerId, dto.DogId, ct);
        
        if (isBlocked)
        {
            throw new ConflictException("This client is on Blacklist!");
        }


        var startTime = dto.Date;
        var endTime = dto.Date.AddMinutes(serviceBreed.Duration);
        
        var visitOverlaps = await ctx.Visits
            .Where(e => e.GroomerId == dto.GroomerId)
            .Where(d => d.SalonId == salonId)
            .Where(d => d.Status != StatusEnum.Cancelled && d.Status != StatusEnum.NoShow)
            .AnyAsync(d => startTime < d.Date.AddMinutes(d.EstimatedDuration)
                           && endTime > d.Date, ct);

        if (visitOverlaps)
            throw new ConflictException("Groomer already has a visit at this time");
        
        
        var timeOffOverlaps = await ctx.GroomerTimeOffs
            .Where(t => t.SalonId == salonId)
            .Where(t => t.GroomerId == dto.GroomerId)
            .AnyAsync(t => startTime < t.EndDate.ToDateTime(t.EndTime)
                           && endTime > t.StartDate.ToDateTime(t.StartTime), ct);

        if (timeOffOverlaps)
            throw new ConflictException("Groomer is unavailable at this time");

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
/*
 
// Zaczątek portalu klienta — nieaktywne.
// Wymaga rejestracji kont klienckich (RegisterClientAccountAsync), której nie ma.
// Logika napisana, nieprzetestowana. Sierpień 2026.
    public async Task<int> BookVisitByClientAsync(int salonId, int userId, AddVisitDto dto, CancellationToken ct)
    {
        
        
        var dogOwnerExist = await ctx.DogOwners
            .Where(u => u.UserId == userId && u.SalonId == salonId)
            .FirstOrDefaultAsync(ct);
        if (dogOwnerExist == null)
        {
            throw new NotFoundException("Dog owner not found");
        }
        
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
        
        if (dog.DogOwnerId != dogOwnerExist.Id)
            throw new NotFoundException("Dog not found");
        
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

        var isBlocked = await BlacklistCheckService.IsBlockedAsync(salonId, dog.DogOwnerId, dto.DogId, ct);
        
        if (isBlocked)
        {
            throw new ConflictException("This client is on Blacklist!");
        }


        var startTime = dto.Date;
        var endTime = dto.Date.AddMinutes(serviceBreed.Duration);
        
        var visitOverlaps = await ctx.Visits
            .Where(e => e.GroomerId == dto.GroomerId)
            .Where(d => d.SalonId == salonId)
            .Where(d => d.Status != StatusEnum.Cancelled && d.Status != StatusEnum.NoShow)
            .AnyAsync(d => startTime < d.Date.AddMinutes(d.EstimatedDuration)
                           && endTime > d.Date, ct);

        if (visitOverlaps)
            throw new ConflictException("Groomer already has a visit at this time");
        
        
        var timeOffOverlaps = await ctx.GroomerTimeOffs
            .Where(t => t.SalonId == salonId)
            .Where(t => t.GroomerId == dto.GroomerId)
            .AnyAsync(t => startTime < t.EndDate.ToDateTime(t.EndTime)
                           && endTime > t.StartDate.ToDateTime(t.StartTime), ct);

        if (timeOffOverlaps)
            throw new ConflictException("Groomer is unavailable at this time");

        
        var date = DateOnly.FromDateTime(dto.Date);
        var requestedTime = TimeOnly.FromDateTime(dto.Date).ToString("HH:mm");

        var availability = await availabilityReaderService
            .GetAvailabilitySlotsAsync(salonId, date, dto.ServiceBreedId, dto.GroomerId, ct);

        var slotAvailable = availability
            .Any(a => a.GroomerId == dto.GroomerId && a.AvailableSlots.Contains(requestedTime));

        if (!slotAvailable)
            throw new ConflictException("Selected time slot is no longer available");
        
        
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
            DogOwnerId = dogOwnerExist.Id,
            GroomerId = dto.GroomerId,
            ServiceBreedId = dto.ServiceBreedId
        };
        
        ctx.Visits.Add(newVisit);
        await ctx.SaveChangesAsync(ct);
        return newVisit.Id;



    }
    */

}