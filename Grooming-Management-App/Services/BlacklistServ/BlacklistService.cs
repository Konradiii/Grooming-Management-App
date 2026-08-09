using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.BlacklistDto;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.BlacklistServ;

public class BlacklistService(GroomingDbContext ctx) : IBlacklistService
{
    public async Task<List<GetAllBlacklistDto>> GetAllClientsOfBlacklistAsync(int salonId, string? phoneNumber, CancellationToken ct)
    {
        return await ctx.Blacklists
            .Where(b => b.SalonId == salonId)
            .Where(b => phoneNumber == null || b.DogOwner.Phone == phoneNumber)
            .Select(e => new GetAllBlacklistDto
            {
                Id = e.Id,
                DogOwnerFullName = e.DogOwner.FirstName + " " + e.DogOwner.LastName,
                DogName = e.Dog != null ? e.Dog.Name : null,
            }).ToListAsync(ct);
        
    }

    public async Task<GetDetailsBlackListDto> GetDetailsBlackListAsync(int salonId, int id, CancellationToken ct)
    {

        var record = await ctx.Blacklists
            .Where(b => b.SalonId == salonId)
            .Where(b => b.Id == id)
            .Select(e=> new GetDetailsBlackListDto
            {
                Id = e.Id,
                DogOwnerFullName = e.DogOwner.FirstName + " " + e.DogOwner.LastName,
                DogName = e.Dog != null ? e.Dog.Name : null,
                Reason =  e.Reason,
                CreatedAt = e.CreatedAt,

            }).FirstOrDefaultAsync(ct);
        if (record == null)
        {
            throw new NotFoundException($"No record with id {id} was found.");
        }

        return record;
    }
    
    public async Task<int> AddToBlacklistByDogOwnerAsync(int salonId, CreateBlacklistByDogOwnerDto dto, CancellationToken ct)
    {
        
        var alreadyBlocked = await ctx.Blacklists
            .AnyAsync(b => b.DogOwnerId == dto.DogOwnerId && b.SalonId == salonId, ct);
        
        if (alreadyBlocked)
        {
            throw new ConflictException("This dog owner is already blacklisted");
        }

        var newRecord = new Blacklist
        {
            Reason = dto.Reason,
            CreatedAt = DateTime.UtcNow,
            SalonId = salonId,
            DogOwnerId = dto.DogOwnerId,
            
        };
        await ctx.Blacklists.AddAsync(newRecord, ct);
        await ctx.SaveChangesAsync(ct);
        return newRecord.Id;
    }
    
    public async Task<int> AddToBlacklistByDogAsync(int salonId, CreateBlacklistByDogDto dto, CancellationToken ct)
    {
        
        var dog = await ctx.Dogs
            .Where(e => e.Id == dto.DogId && e.SalonId == salonId)
            .FirstOrDefaultAsync(ct);

        if (dog == null)
        {
            throw new NotFoundException("Dog not found");
        }
        
        
        var alreadyBlocked = await ctx.Blacklists
            .AnyAsync(b => b.DogId == dto.DogId && b.SalonId == salonId, ct);
        
        if (alreadyBlocked)
        {
            throw new ConflictException("This dog is already blacklisted");
        }
        
        var newRecord = new Blacklist
        {
            Reason = dto.Reason,
            CreatedAt = DateTime.UtcNow,
            SalonId = salonId,
            DogOwnerId = dog.DogOwnerId,
            DogId = dto.DogId,
            
            
        };
        await ctx.Blacklists.AddAsync(newRecord, ct);
        await ctx.SaveChangesAsync(ct);
        return newRecord.Id;
        
    }
    
    public async Task DeleteRecordFromBlacklistAsync(int salonId, int id, CancellationToken ct)
    {
        
        var record = await ctx.Blacklists
            .Where(b => b.SalonId == salonId)
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync(ct);
        if (record == null)
        {
            throw new NotFoundException($"No record with id {id} was found.");
        }
        ctx.Blacklists.Remove(record);
        await ctx.SaveChangesAsync(ct);
        
    }
    
    public async Task<bool> IsBlockedAsync(int salonId, int dogOwnerId, int? dogId, CancellationToken ct)
    {
        
        var dogOwnerExist = await ctx.Blacklists
            .Where(e=>e.SalonId == salonId)
            .Where(e=> dogOwnerId ==e.DogOwnerId)
            .AnyAsync(ct);
        
        var dogExist = dogId != null && await ctx.Blacklists
            .Where(e => e.SalonId == salonId)
            .Where(e => e.DogId == dogId)
            .AnyAsync(ct);
        
        bool result = (dogOwnerExist || dogExist);
        
        return result;
        
    }
}