using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.GroomerTimeOffDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.GroomerTimeOffServ;

public class GroomerTimeOffService(GroomingDbContext ctx) : IGroomerTimeOffReaderService, IGroomerTimeOffWriterService
{
    // Grafik i blokady opisują czas lokalny salonu (DateOnly + TimeOnly),
    // a Visit.Date jest w UTC. Konwersja potrzebna przy porównywaniu.
    private static readonly TimeZoneInfo PolishTime =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

    public async Task<int> CreateGroomerTimeOffAsync(int salonId, CreateGroomerTimeOffDto dto, CancellationToken ct)
    {
        if (dto.StartDate > dto.EndDate)
        {
            throw new ConflictException(ErrorCodes.InvalidDateRange);
        }

        if (dto.StartTime >= dto.EndTime)
        {
            throw new ConflictException(ErrorCodes.InvalidTimeRange);
        }

        var groomerExists = await ctx.Groomers
            .AnyAsync(g => g.Id == dto.GroomerId && g.SalonId == salonId, ct);

        if (!groomerExists)
        {
            throw new NotFoundException(ErrorCodes.GroomerNotFound);
        }

        var startUtc = TimeZoneInfo.ConvertTimeToUtc(dto.StartDate.ToDateTime(dto.StartTime), PolishTime);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(dto.EndDate.ToDateTime(dto.EndTime), PolishTime);

        var hasConflictingVisits = await ctx.Visits
            .Where(v => v.SalonId == salonId)
            .Where(v => v.GroomerId == dto.GroomerId)
            .Where(v => v.Status != StatusEnum.Cancelled && v.Status != StatusEnum.NoShow)
            .AnyAsync(v => v.Date < endUtc
                           && v.Date.AddMinutes(v.EstimatedDuration) > startUtc, ct);

        if (hasConflictingVisits)
        {
            throw new ConflictException(ErrorCodes.TimeOffHasVisits);
        }

        var newTimeOff = new GroomerTimeOff
        {
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Reason = dto.Reason,
            CreatedAt = DateTime.UtcNow,
            SalonId = salonId,
            GroomerId = dto.GroomerId
        };

        ctx.GroomerTimeOffs.Add(newTimeOff);
        await ctx.SaveChangesAsync(ct);

        return newTimeOff.Id;
    }

    public async Task<GetGroomerTimeOffDto> GetGroomerTimeOffAsync(int salonId, int timeOffId, CancellationToken ct)
    {
        var timeOff = await ctx.GroomerTimeOffs
            .Where(t => t.SalonId == salonId)
            .Where(t => t.Id == timeOffId)
            .Select(t => new GetGroomerTimeOffDto
            {
                Id = t.Id,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                StartTime = t.StartTime,
                EndTime = t.EndTime,
                Reason = t.Reason,
                GroomerId = t.GroomerId,
                GroomerFullName = t.Groomer.FirstName + " " + t.Groomer.LastName
            })
            .FirstOrDefaultAsync(ct);

        if (timeOff == null)
        {
            throw new NotFoundException(ErrorCodes.TimeOffNotFound);
        }

        return timeOff;
    }

    public async Task<List<GetGroomerTimeOffDto>> GetAllGroomerTimeOffsAsync(int salonId, int? groomerId, DateOnly? dateFrom, DateOnly? dateTo, CancellationToken ct)
    {
        return await ctx.GroomerTimeOffs
            .Where(t => t.SalonId == salonId)
            .Where(t => groomerId == null || t.GroomerId == groomerId)
            .Where(t => dateFrom == null || t.EndDate >= dateFrom)
            .Where(t => dateTo == null || t.StartDate <= dateTo)
            .Select(t => new GetGroomerTimeOffDto
            {
                Id = t.Id,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                StartTime = t.StartTime,
                EndTime = t.EndTime,
                Reason = t.Reason,
                GroomerId = t.GroomerId,
                GroomerFullName = t.Groomer.FirstName + " " + t.Groomer.LastName
            })
            .ToListAsync(ct);
    }

    public async Task DeleteGroomerTimeOffAsync(int salonId, int timeOffId, CancellationToken ct)
    {
        var timeOff = await ctx.GroomerTimeOffs
            .Where(t => t.SalonId == salonId)
            .Where(t => t.Id == timeOffId)
            .FirstOrDefaultAsync(ct);

        if (timeOff == null)
        {
            throw new NotFoundException(ErrorCodes.TimeOffNotFound);
        }

        ctx.GroomerTimeOffs.Remove(timeOff);
        await ctx.SaveChangesAsync(ct);
    }
}