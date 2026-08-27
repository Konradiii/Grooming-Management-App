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
            MaxBookingDaysAhead = salonInfo.MaxBookingDaysAhead,
            RemindersEnabled = salonInfo.RemindersEnabled,
            ReminderHoursBefore = salonInfo.ReminderHoursBefore
        };


    }


    public async Task UpdateSalonAsync(UpdateSalonDto dto, int salonId, CancellationToken ct)
    {
        Validate.NotEmpty(dto.Name, ErrorCodes.NameRequired);
        Validate.PolishPostalCode(dto.PostalCode);

        if (dto.ReminderHoursBefore < 1 || dto.ReminderHoursBefore > 168)
        {
            throw new ConflictException(ErrorCodes.InvalidReminderSettings);
        }

        var salonInfo = await ctx.Salons.FirstOrDefaultAsync(s => s.Id == salonId, ct);

        if (salonInfo == null)
        {
            throw new NotFoundException(ErrorCodes.SalonNotFound);
        }

        salonInfo.Name = dto.Name.Trim();
        salonInfo.Street = dto.Street?.Trim();
        salonInfo.BuildingNumber = dto.BuildingNumber?.Trim();
        salonInfo.ApartmentNumber = dto.ApartmentNumber?.Trim();
        salonInfo.PostalCode = Validate.NormalizePostalCode(dto.PostalCode);
        salonInfo.City = dto.City?.Trim();
        salonInfo.RemindersEnabled = dto.RemindersEnabled;
        salonInfo.ReminderHoursBefore = dto.ReminderHoursBefore;

        await ctx.SaveChangesAsync(ct);
    }
}