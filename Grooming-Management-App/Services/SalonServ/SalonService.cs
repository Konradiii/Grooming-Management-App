using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.SalonDTO;
using Grooming_Management_App.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.SalonServ;

public class SalonService(GroomingDbContext ctx) : ISalonService
{
    public async Task<GetSalonDto> GetSalonAsync(int salonId, CancellationToken ct)
    {
    
        var salonInfo = await ctx.Salons.FirstOrDefaultAsync(s => s.Id == salonId, ct);
        if (salonInfo == null)
        {
            throw new NotFoundException("Salon not found");
        }

        return new GetSalonDto
        {
            Id = salonInfo.Id,
            Name = salonInfo.Name,
        };


    }
    
    
    public async Task UpdateSalonAsync(UpdateSalonDto dto, int salonId, CancellationToken ct)
    {
        var salonInfo = await ctx.Salons.FirstOrDefaultAsync(s => s.Id == salonId, ct);
        if (salonInfo == null)
        {
            throw new NotFoundException("Salon not found");
        }
        salonInfo.Name = dto.Name;
        
        await ctx.SaveChangesAsync(ct);        
    }
}