using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.WaitlistDTO;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.WaitlistServ;

public class WaitlistService(GroomingDbContext ctx) : IWaitlistReaderService, IWaitlistWriterService 
{
    public async Task<int> AddToWaitlistAsync(int salonId, CreateWaitlistDto dto, CancellationToken ct)
    {
        
        var recoedExist = await ctx.Waitlists.AnyAsync(w => 
            w.SalonId == salonId 
            && w.DogOwnerId == dto.DogOwnerId, 
            ct);

        if (recoedExist)
        {
            throw new ConflictException(ErrorCodes.ClientAlreadyOnWaitlist);
        }
        
        var newRecord = new Waitlist
        {
            CreatedAt = DateTime.UtcNow,
            Priority = dto.Priority,
            SalonId = salonId,
            DogOwnerId = dto.DogOwnerId,
            DogId = dto.DogId
        };
        
        await ctx.Waitlists.AddAsync(newRecord, ct);
        await ctx.SaveChangesAsync(ct);
        return newRecord.Id;
    }
    
    public async Task RemoveFromWaitlistAsync(int salonId, int id, CancellationToken ct)
    {
        var recordExist = await ctx.Waitlists
            .Where(x => x.Id == id)
            .Where(x => x.SalonId == salonId)
            .FirstOrDefaultAsync(ct);

        if (recordExist == null)
        {
            throw new NotFoundException(ErrorCodes.WaitlistRecordNotFound); 
        }
        
        ctx.Waitlists.Remove(recordExist);
        await ctx.SaveChangesAsync(ct);

    }
    
    public async Task<List<GetWaitlistDto>> GetAllWaitlistAsync(int salonId, CancellationToken ct)
    {
        return await ctx.Waitlists
            .Where(x => x.SalonId == salonId)
            .Select(e=> new GetWaitlistDto
            {
                Id =  e.Id,
                CreatedAt = e.CreatedAt,
                Priority = e.Priority,
                DogOwnerFullName = e.DogOwner.FirstName + " " + e.DogOwner.LastName,
                DogName = e.Dog != null ? e.Dog.Name : null,
                DogOwnerPhone = e.DogOwner.Phone,
            }).ToListAsync(ct);
    }
}