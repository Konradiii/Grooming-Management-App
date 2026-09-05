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
            Phone = salonInfo.Phone,
            Street = salonInfo.Street,
            BuildingNumber = salonInfo.BuildingNumber,
            ApartmentNumber = salonInfo.ApartmentNumber,
            PostalCode = salonInfo.PostalCode,
            City = salonInfo.City,
            MinBookingHoursAhead = salonInfo.MinBookingHoursAhead,
            MaxBookingDaysAhead = salonInfo.MaxBookingDaysAhead,
            RemindersEnabled = salonInfo.RemindersEnabled,
            ReminderHoursBefore = salonInfo.ReminderHoursBefore,
            SmsIncluded = salonInfo.SmsIncluded,
            SmsPurchased = salonInfo.SmsPurchased,
            SmsResetDate = salonInfo.SmsResetDate,
        };


    }


    public async Task UpdateSalonAsync(UpdateSalonDto dto, int salonId, CancellationToken ct)
    {
        Validate.NotEmpty(dto.Name, ErrorCodes.NameRequired);
        Validate.PolishPostalCode(dto.PostalCode);
        if (!string.IsNullOrWhiteSpace(dto.Phone))
            Validate.PolishPhone(dto.Phone);

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
    
    public async Task<GetSmsBalanceDto> GetSmsBalanceAsync(int salonId, CancellationToken ct)
    {
        var balance = await ctx.Salons
            .Where(s => s.Id == salonId)
            .Select(s => new GetSmsBalanceDto
            {
                Remaining = s.SmsIncluded + s.SmsPurchased,
                ResetDate = s.SmsResetDate
            })
            .FirstOrDefaultAsync(ct);

        if (balance == null)
        {
            throw new NotFoundException(ErrorCodes.SalonNotFound);
        }

        return balance;
    }
}