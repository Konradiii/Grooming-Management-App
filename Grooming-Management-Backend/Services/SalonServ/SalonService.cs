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
            throw new NotFoundException(ErrorCodes.SalonNotFound);
        }

        return new GetSalonDto
        {
            Id = salonInfo.Id,
            Name = salonInfo.Name,
            Street = salonInfo.Street,
            BuildingNumber = salonInfo.BuildingNumber,
            ApartmentNumber = salonInfo.ApartmentNumber,
            PostalCode = salonInfo.PostalCode,
            City = salonInfo.City,
            MinBookingHoursAhead = salonInfo.MinBookingHoursAhead,
            MaxBookingDaysAhead = salonInfo.MaxBookingDaysAhead
        };


    }
    
    
    public async Task UpdateSalonAsync(UpdateSalonDto dto, int salonId, CancellationToken ct)
    {
        
        if (dto.MinBookingHoursAhead < 0)
        {
            throw new ConflictException(ErrorCodes.InvalidBookingSettings);
        }

        if (dto.MaxBookingDaysAhead <= 0)
        {
            throw new ConflictException(ErrorCodes.InvalidBookingSettings);
        }
        var salonInfo = await ctx.Salons.FirstOrDefaultAsync(s => s.Id == salonId, ct);
        
        if (salonInfo == null)
        {
            throw new NotFoundException(ErrorCodes.SalonNotFound);
        }
        
        salonInfo.Name = dto.Name;
        salonInfo.Street = dto.Street;
        salonInfo.BuildingNumber = dto.BuildingNumber;
        salonInfo.ApartmentNumber = dto.ApartmentNumber;
        salonInfo.PostalCode = dto.PostalCode;
        salonInfo.City = dto.City;
        salonInfo.MinBookingHoursAhead = dto.MinBookingHoursAhead;
        salonInfo.MaxBookingDaysAhead = dto.MaxBookingDaysAhead;
        
        await ctx.SaveChangesAsync(ct);        
    }
}