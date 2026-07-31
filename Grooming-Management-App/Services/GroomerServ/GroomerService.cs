using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.GroomerDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.GroomerServ;

public class GroomerService(GroomingDbContext ctx) : IGroomerService
{
    public async Task DeactivateGroomerAsync(int id, int salonId, CancellationToken ct)
    {
        var groomer = await ctx.Groomers
            .Where(g => g.Id == id && g.SalonId == salonId)
            .FirstOrDefaultAsync(ct);

        if (groomer == null)
        {
            throw new NotFoundException("Groomer not found");
        }

        if (groomer.ActiveStatus == ActiveStatusEnum.Inactive)
        {
            return;
        }
        
        groomer.ActiveStatus = ActiveStatusEnum.Inactive;
        await ctx.SaveChangesAsync(ct);
        
    }
    
    public async Task ActivateGroomerAsync(int id, int salonId, CancellationToken ct)
    {
        
        var groomer = await ctx.Groomers
            .Where(g => g.Id == id && g.SalonId == salonId)
            .FirstOrDefaultAsync(ct);

        if (groomer == null)
        {
            throw new NotFoundException("Groomer not found");
        }

        if (groomer.ActiveStatus == ActiveStatusEnum.Active)
        {
            return;
        }
        
        groomer.ActiveStatus = ActiveStatusEnum.Active;
        await ctx.SaveChangesAsync(ct);
        
    }
    
    public async Task<GetGroomerDto> GetGroomerAsync(int id, int salonId, CancellationToken ct)
    {
        
        var result = await ctx.Groomers
            .Where(g => g.Id == id && g.SalonId == salonId)
            .Select(e=> new GetGroomerDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                ActiveStatus = e.ActiveStatus,
            }).FirstOrDefaultAsync(ct);
        if (result == null)
        {
            throw new NotFoundException("Groomer not found");
        }
        return result;
        
    }
    public async Task<List<GetGroomerDto>> GetAllGroomersAsync(int salonId, CancellationToken ct)
    {
        
        return await ctx.Groomers
            .Where(g => g.SalonId == salonId)
            .Select(e => new GetGroomerDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                ActiveStatus = e.ActiveStatus,
            }).ToListAsync(ct);
        
    }
    
    public async Task EditGroomerAsync(EditGroomerDto dto, int id, int salonId,  CancellationToken ct)
    {
        var edited = await ctx.Groomers
            .Where(g => g.Id == id && g.SalonId == salonId)
            .FirstOrDefaultAsync(ct);
        if (edited == null)
        {
            throw new NotFoundException("Groomer not found");
        }
        edited.FirstName = dto.FirstName;
        edited.LastName = dto.LastName;
        
        await ctx.SaveChangesAsync(ct);
        
    }
    
    public async Task CreateGroomerAsync(CreateGroomerDto dto, int salonId, CancellationToken ct)
    {
        ctx.Groomers.Add(new Groomer 
            { 
                SalonId = salonId,
                FirstName = dto.FirstName,
                LastName = dto.LastName ,
                ActiveStatus = ActiveStatusEnum.Active
            });
        await ctx.SaveChangesAsync(ct);

    }
}