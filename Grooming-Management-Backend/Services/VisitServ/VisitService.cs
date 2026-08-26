using System.Xml;
using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.VisitDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Grooming_Management_App.Services.AvailabilityServ;
using Grooming_Management_App.Services.BlacklistServ;
using Grooming_Management_App.Services.CurrentUserServ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.VisitServ;

public class VisitService(GroomingDbContext ctx, IBlacklistCheckService blacklistCheckService, ICurrentUserService currentUser) : IVisitReaderService, IVisitWriterService
{
    public async Task<List<GetAllVisitsDto>> GetAllVisitsAsync(int salonId, VisitFilterDto filter, CancellationToken ct)
    {
        var query = ctx.Visits
            .Where(v => v.SalonId == salonId)
            .Where(v => filter.GroomerId == null || filter.GroomerId == v.GroomerId)
            .Where(e => filter.DateFrom == null || e.Date >= filter.DateFrom)
            .Where(e => filter.DateTo == null || e.Date <= filter.DateTo)
            .Where(v => filter.DogId == null || v.DogId == filter.DogId)
            .Where(v => filter.Status == null || v.Status == filter.Status);

        if (currentUser.Role == RoleEnum.Groomer)
        {
            var me = await GetCurrentGroomerAsync(salonId, ct);

            if (me == null)
                return new List<GetAllVisitsDto>();

            if (!me.CanSeeAllVisits)
            {
                var myId = me.Id;
                query = query.Where(v => v.GroomerId == myId || v.AssistantGroomerId == myId);
            }
        }

        return await query
            .Select(v => new GetAllVisitsDto
            {
                Id = v.Id,
                Date = v.Date,
                DogName = v.Dog.Name,
                GroomerId = v.GroomerId,
                GroomerName = v.Groomer.FirstName + " " + v.Groomer.LastName,
                ServiceName = v.ServiceBreed.Service.Name,
                EstimatedDuration = v.EstimatedDuration,
                Status = v.Status,
                BreedName = v.Dog.Breed.Name,
                AssistantGroomerFullName = v.AssistantGroomer != null
                    ? v.AssistantGroomer.FirstName + " " + v.AssistantGroomer.LastName
                    : null,
            })
            .ToListAsync(ct);
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
                BreedName =  v.ServiceBreed.Breed.Name,
                AssistantGroomerFullName = v.AssistantGroomer != null
                    ? v.AssistantGroomer.FirstName + " " + v.AssistantGroomer.LastName
                    : null,
                
            }).FirstOrDefaultAsync(ct);

        if (visit == null)
        {
            throw new NotFoundException(ErrorCodes.VisitNotFound);
        }
        
        return visit;

    }
    
    public async Task<int> AddVisitAsync(int salonId, AddVisitDto dto, CancellationToken ct)
    {
        if (currentUser.Role == RoleEnum.Groomer)
        {
            var me = await GetCurrentGroomerAsync(salonId, ct);

            if (me == null || !me.CanCreateVisits)
                throw new ForbiddenException(ErrorCodes.NoPermissionToCreateVisits);
        }
        
        
        if (dto.DurationMinutes is <= 0)
        {
            throw new ConflictException(ErrorCodes.InvalidDuration);
        }

        var serviceBreed = await ctx.ServiceBreeds
            .Where(g => salonId == g.SalonId)
            .Where(d => d.Id == dto.ServiceBreedId)
            .FirstOrDefaultAsync(ct);
        
        if (serviceBreed == null)
        {
            throw new NotFoundException(ErrorCodes.ServiceBreedNotFound);
        }
        
        var dog = await ctx.Dogs
            .Where(g => salonId == g.SalonId)
            .Where(d => d.Id == dto.DogId)
            .FirstOrDefaultAsync(ct);
        
        if (dog == null)
        {
            throw new NotFoundException(ErrorCodes.DogNotFound);
        }
        
        var groomer = await ctx.Groomers
            .Where(g => salonId == g.SalonId)
            .Where(g => g.Id == dto.GroomerId)
            .FirstOrDefaultAsync(ct);

        if (groomer == null)
        {
            throw new NotFoundException(ErrorCodes.GroomerNotFound);
        }

        if (serviceBreed.BreedId != dog.BreedId)
        {
            throw new ConflictException(ErrorCodes.ServiceBreedNotFound);
        }
        
        var duplicateExists = await ctx.Visits
            .AnyAsync(v => v.DogId == dto.DogId 
                           && v.Date == dto.Date 
                           && v.SalonId == salonId, ct);
        if (duplicateExists)
        {
            throw new ConflictException(ErrorCodes.DuplicateVisit);
        }
        
        if (dto.AssistantGroomerId != null)
        {
            if (dto.AssistantGroomerId == dto.GroomerId)
                throw new ConflictException(ErrorCodes.AssistantMustDiffer);

            var assistantExists = await ctx.Groomers
                .AnyAsync(g => g.Id == dto.AssistantGroomerId && g.SalonId == salonId, ct);

            if (!assistantExists)
                throw new NotFoundException(ErrorCodes.AssistantNotFound);
        }

        var isBlocked = await blacklistCheckService.IsBlockedAsync(salonId, dog.DogOwnerId, dto.DogId, ct);
        
        if (isBlocked)
        {
            throw new ConflictException(ErrorCodes.ClientBlacklisted);
        }


        var duration = dto.DurationMinutes ?? serviceBreed.Duration;

        var startTime = dto.Date;
        var endTime = dto.Date.AddMinutes(duration);
        
        var visitOverlaps = await ctx.Visits
            .Where(e => e.GroomerId == dto.GroomerId)
            .Where(d => d.SalonId == salonId)
            .Where(d => d.Status != StatusEnum.Cancelled && d.Status != StatusEnum.NoShow)
            .AnyAsync(d => startTime < d.Date.AddMinutes(d.EstimatedDuration)
                           && endTime > d.Date, ct);

        if (visitOverlaps)
            throw new ConflictException(ErrorCodes.VisitOverlaps);
        
        
        var timeOffOverlaps = await ctx.GroomerTimeOffs
            .Where(t => t.SalonId == salonId)
            .Where(t => t.GroomerId == dto.GroomerId)
            .AnyAsync(t => startTime < t.EndDate.ToDateTime(t.EndTime)
                           && endTime > t.StartDate.ToDateTime(t.StartTime), ct);

        if (timeOffOverlaps)
            throw new ConflictException(ErrorCodes.GroomerUnavailable);

        var newVisit = new Visit
        {
            CreatedAt = DateTime.UtcNow,
            Date = dto.Date,
            EstimatedDuration = duration,
            ProposedPrice = serviceBreed.Price,
            Status = StatusEnum.Scheduled,
            Notes = dto.Notes,
            SalonId = salonId,
            DogId = dto.DogId,
            DogOwnerId = dog.DogOwnerId,
            GroomerId = dto.GroomerId,
            ServiceBreedId = dto.ServiceBreedId,
            SettlementType = groomer.SettlementType,
            SettlementRate = groomer.SettlementRate,
            AssistantGroomerId = dto.AssistantGroomerId
        };
        
        ctx.Visits.Add(newVisit);
        await ctx.SaveChangesAsync(ct);
        return newVisit.Id;
        
        
    }
    
    
    public async Task EditVisitAsync(int salonId, int visitId, EditVisitDto dto, CancellationToken ct)
    {
        var visit = await ctx.Visits
            .Where(v => v.SalonId == salonId && v.Id == visitId)
            .FirstOrDefaultAsync(ct);

        if (visit == null)
        {
            throw new NotFoundException(ErrorCodes.VisitNotFound);
        }

        var groomerExist = await ctx.Groomers
            .Where(g => salonId == g.SalonId && g.Id == dto.GroomerId)
            .AnyAsync(ct);

        if (!groomerExist)
        {
            throw new NotFoundException(ErrorCodes.GroomerNotFound);
        }

        var startTime = dto.Date;
        var endTime = dto.Date.AddMinutes(visit.EstimatedDuration);

        var visitOverlaps = await ctx.Visits
            .Where(d => d.SalonId == salonId)
            .Where(d => d.GroomerId == dto.GroomerId)
            .Where(d => d.Id != visitId)
            .Where(d => d.Status != StatusEnum.Cancelled && d.Status != StatusEnum.NoShow)
            .AnyAsync(d => startTime < d.Date.AddMinutes(d.EstimatedDuration)
                           && endTime > d.Date, ct);

        if (visitOverlaps)
            throw new ConflictException(ErrorCodes.VisitOverlaps);

        var timeOffOverlaps = await ctx.GroomerTimeOffs
            .Where(t => t.SalonId == salonId)
            .Where(t => t.GroomerId == dto.GroomerId)
            .AnyAsync(t => startTime < t.EndDate.ToDateTime(t.EndTime)
                           && endTime > t.StartDate.ToDateTime(t.StartTime), ct);

        if (timeOffOverlaps)
            throw new ConflictException(ErrorCodes.GroomerUnavailable);

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
            throw new NotFoundException(ErrorCodes.VisitNotFound);
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
            throw new NotFoundException(ErrorCodes.VisitNotFound);
        }
        visit.FinalPrice = finalPrice;
        await ctx.SaveChangesAsync(ct);
        
        
    }
    
public async Task<int> CreateVisitWithNewDogAsync(int salonId, CreateVisitWithNewDogDto dto, CancellationToken ct)
{
    if (currentUser.Role == RoleEnum.Groomer)
    {
        var me = await GetCurrentGroomerAsync(salonId, ct);

        if (me == null || !me.CanCreateVisits)
            throw new ForbiddenException(ErrorCodes.NoPermissionToCreateVisits);
    }
    
    Validate.NotEmpty(dto.FirstName, ErrorCodes.NameRequired);
    Validate.NotEmpty(dto.LastName, ErrorCodes.NameRequired);
    Validate.NotEmpty(dto.DogName, ErrorCodes.NameRequired);
    Validate.PolishPhone(dto.Phone);
    

    if (dto.DurationMinutes is <= 0)
    {
        throw new ConflictException(ErrorCodes.InvalidDuration);
    }

    var blacklistedByPhone = await ctx.Blacklists
        .Where(b => b.SalonId == salonId)
        .AnyAsync(b => b.DogOwner.Phone == dto.Phone, ct);

    if (blacklistedByPhone)
    {
        throw new ConflictException(ErrorCodes.ClientBlacklisted);
    }

    var ownerExists = await ctx.DogOwners
        .AnyAsync(o => o.Phone == dto.Phone && o.SalonId == salonId, ct);

    if (ownerExists)
    {
        throw new ConflictException(ErrorCodes.PhoneTaken);
    }

    var breedExists = await ctx.Breeds.AnyAsync(b => b.Id == dto.BreedId, ct);

    if (!breedExists)
    {
        throw new NotFoundException(ErrorCodes.BreedNotFound);
    }

    var serviceBreed = await ctx.ServiceBreeds
        .Where(s => s.Id == dto.ServiceBreedId && s.SalonId == salonId)
        .FirstOrDefaultAsync(ct);

    if (serviceBreed == null)
    {
        throw new NotFoundException(ErrorCodes.ServiceBreedNotFound);
    }

    if (serviceBreed.BreedId != dto.BreedId)
    {
        throw new ConflictException(ErrorCodes.BreedMismatch);
    }

    var groomer = await ctx.Groomers
        .Where(g => g.SalonId == salonId)
        .Where(g => g.Id == dto.GroomerId)
        .FirstOrDefaultAsync(ct);

    if (groomer == null)
    {
        throw new NotFoundException(ErrorCodes.GroomerNotFound);
    }

    if (dto.AssistantGroomerId != null)
    {
        if (dto.AssistantGroomerId == dto.GroomerId)
            throw new ConflictException(ErrorCodes.AssistantMustDiffer);

        var assistantExists = await ctx.Groomers
            .AnyAsync(g => g.Id == dto.AssistantGroomerId && g.SalonId == salonId, ct);

        if (!assistantExists)
            throw new NotFoundException(ErrorCodes.AssistantNotFound);
    }

    var duration = dto.DurationMinutes ?? serviceBreed.Duration;
    var startTime = dto.Date;
    var endTime = dto.Date.AddMinutes(duration);

    var visitOverlaps = await ctx.Visits
        .Where(v => v.GroomerId == dto.GroomerId)
        .Where(v => v.SalonId == salonId)
        .Where(v => v.Status != StatusEnum.Cancelled && v.Status != StatusEnum.NoShow)
        .AnyAsync(v => startTime < v.Date.AddMinutes(v.EstimatedDuration)
                    && endTime > v.Date, ct);

    if (visitOverlaps)
    {
        throw new ConflictException(ErrorCodes.VisitOverlaps);
    }

    var timeOffOverlaps = await ctx.GroomerTimeOffs
        .Where(t => t.SalonId == salonId)
        .Where(t => t.GroomerId == dto.GroomerId)
        .AnyAsync(t => startTime < t.EndDate.ToDateTime(t.EndTime)
                    && endTime > t.StartDate.ToDateTime(t.StartTime), ct);

    if (timeOffOverlaps)
    {
        throw new ConflictException(ErrorCodes.GroomerUnavailable);
    }

    var owner = new Models.DogOwner
    {
        FirstName = dto.FirstName.Trim(),
        LastName = dto.LastName.Trim(),
        Phone = dto.Phone.Trim(),
        SalonId = salonId
    };

    var dog = new Dog
    {
        Name = dto.DogName.Trim(),
        AgeInMonths = dto.AgeInMonths,
        BreedId = dto.BreedId,
        Notes = dto.DogNotes,
        SalonId = salonId
    };

    owner.Dogs.Add(dog);

    var visit = new Visit
    {
        CreatedAt = DateTime.UtcNow,
        Date = dto.Date,
        EstimatedDuration = duration,
        ProposedPrice = serviceBreed.Price,
        Status = StatusEnum.Scheduled,
        Notes = dto.Notes,
        SalonId = salonId,
        Dog = dog,
        DogOwner = owner,
        GroomerId = dto.GroomerId,
        ServiceBreedId = dto.ServiceBreedId,
        SettlementType = groomer.SettlementType,
        SettlementRate = groomer.SettlementRate,
        AssistantGroomerId = dto.AssistantGroomerId
    };

    ctx.DogOwners.Add(owner);
    ctx.Visits.Add(visit);

    await ctx.SaveChangesAsync(ct);

    return visit.Id;
}
    
    private async Task<Groomer?> GetCurrentGroomerAsync(int salonId, CancellationToken ct)
    {
        return await ctx.Groomers
            .FirstOrDefaultAsync(g => g.SalonId == salonId
                                      && g.UserId == currentUser.UserId, ct);
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
            throw new ConflictException("Service Breed doesn't exist for that breed");
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