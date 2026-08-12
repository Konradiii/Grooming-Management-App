using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.GroomerScheduleDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.GroomerScheduleServ;

public class GroomerScheduleService(GroomingDbContext ctx) : IGroomerScheduleService
{
    public async Task<int> CreateGroomerScheduleAsync(int salonId, CreateGroomerScheduleDto dto, CancellationToken ct)
    {
        if (dto.StartTime >= dto.EndTime)
        {
            throw new ConflictException("Start time must be earlier than end time");
        }
        
        var groomerExists = await ctx.Groomers
            .AnyAsync(g => g.Id == dto.GroomerId && g.SalonId == salonId, ct);

        if (!groomerExists)
        {
            throw new NotFoundException("Groomer not found");
        }

        var overlaps = await ctx.GroomerSchedules
            .AnyAsync(s => 
                s.SalonId == salonId
                && s.GroomerId == dto.GroomerId
                && s.DayOfWeek == dto.DayOfWeek
                && dto.StartTime < s.EndTime
                && dto.EndTime > s.StartTime, ct
            );
        
        if (overlaps)
        {
            throw new ConflictException("This time range overlaps with an existing schedule");
        }

        
        var newSchedule = new GroomerSchedule
        {
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            SalonId = salonId,
            GroomerId = dto.GroomerId
        };
        ctx.GroomerSchedules.Add(newSchedule);
        await ctx.SaveChangesAsync(ct);
        
        return newSchedule.Id;
    }
    
    public async Task<GetGroomerScheduleDto> GetGroomerScheduleAsync(int salonId, int groomerScheduleId, CancellationToken ct)
    {
        var gettedSchedule = await ctx.GroomerSchedules
            .Where(s => s.SalonId == salonId)
            .Where(s => s.Id == groomerScheduleId)
            .Select(e => new GetGroomerScheduleDto
            {
                Id = e.Id,
                DayOfWeek = e.DayOfWeek,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                GroomerId = e.GroomerId,
            }).FirstOrDefaultAsync(ct);

        if (gettedSchedule == null)
        {
            throw new NotFoundException("Groomer schedule not found");
        }

        return gettedSchedule;

    }
    
    public async Task<List<GetGroomerScheduleDto>> GetAllGroomerScheduleAsync(int salonId, int? groomerId, DayOfWeekEnum? day, CancellationToken ct)
    {
        
        var schedules = await ctx.GroomerSchedules
            .Where(s => s.SalonId == salonId)
            .Where(s => groomerId == null || s.GroomerId == groomerId)
            .Where(s => day == null || s.DayOfWeek == day)
            .Select(e => new GetGroomerScheduleDto
        {
            Id = e.Id,
            DayOfWeek = e.DayOfWeek,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            GroomerId = e.GroomerId,
        }).ToListAsync(ct);


        return schedules;
        
    }
    
    public async Task DeleteGroomerScheduleAsync(int salonId, int groomerScheduleId, CancellationToken ct)
    {
        var schedule = await ctx.GroomerSchedules
            .Where(s => s.SalonId == salonId)
            .Where(s => s.Id == groomerScheduleId)
            .FirstOrDefaultAsync(ct);

        if (schedule == null)
        {
            throw new NotFoundException("Groomer schedule not found");
        }
        
        ctx.GroomerSchedules.Remove(schedule);
        await ctx.SaveChangesAsync(ct);
        
    }
}