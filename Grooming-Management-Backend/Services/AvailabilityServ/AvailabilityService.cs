using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.AvailabilityDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.AvailabilityServ;

public class AvailabilityService(GroomingDbContext ctx) : IAvailabilityReaderService
{
    private const int SlotLengthMinutes = 15;

    public async Task<List<GetAvailabilityDto>> GetAvailabilitySlotsAsync(
        int salonId, DateOnly date, int serviceBreedId, int? groomerId, CancellationToken ct)
    {
        var serviceBreed = await ctx.ServiceBreeds
            .Where(e => e.Id == serviceBreedId && e.SalonId == salonId)
            .FirstOrDefaultAsync(ct);
        
        if (serviceBreed == null)
            throw new NotFoundException("Service breed not found");

        var duration = serviceBreed.Duration;

        if (groomerId != null)
        {
            var groomerExists = await ctx.Groomers
                .AnyAsync(e => e.Id == groomerId && e.SalonId == salonId, ct);
            if (!groomerExists)
                throw new NotFoundException("Groomer not found");
        }

        // 3. ustawienia rezerwacji salonu
        var salon = await ctx.Salons
            .Where(s => s.Id == salonId)
            .FirstOrDefaultAsync(ct);
        if (salon == null)
            throw new NotFoundException("Salon not found");

        var now = DateTime.UtcNow;
        var minBookingTime = now.AddHours(salon.MinBookingHoursAhead);
        var maxBookingTime = now.AddDays(salon.MaxBookingDaysAhead);

        // 4. groomerzy, dla których liczymy
        var groomers = await ctx.Groomers
            .Where(g => g.SalonId == salonId)
            .Where(g => groomerId == null || g.Id == groomerId)
            .Where(g => groomerId != null || g.ActiveStatus == ActiveStatusEnum.Active)
            .Select(g => new { g.Id, FullName = g.FirstName + " " + g.LastName })
            .ToListAsync(ct);

        if (groomers.Count == 0)
            return new List<GetAvailabilityDto>();

        var groomerIds = groomers.Select(g => g.Id).ToList();
        var dayOfWeek = (DayOfWeekEnum)date.DayOfWeek;

        // 5. grafiki na ten dzień tygodnia — JEDNO zapytanie dla wszystkich groomerów
        var schedules = await ctx.GroomerSchedules
            .Where(s => s.SalonId == salonId)
            .Where(s => groomerIds.Contains(s.GroomerId))
            .Where(s => s.DayOfWeek == dayOfWeek)
            .Select(s => new { s.GroomerId, s.StartTime, s.EndTime })
            .ToListAsync(ct);

        // 6. wizyty tego dnia — Cancelled i NoShow NIE blokują terminu
        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = dayStart.AddDays(1);

        var visits = await ctx.Visits
            .Where(v => v.SalonId == salonId)
            .Where(v => groomerIds.Contains(v.GroomerId))
            .Where(v => v.Date >= dayStart && v.Date < dayEnd)
            .Where(v => v.Status != StatusEnum.Cancelled && v.Status != StatusEnum.NoShow)
            .Select(v => new { v.GroomerId, v.Date, v.EstimatedDuration })
            .ToListAsync(ct);

        // 7. blokady obejmujące ten dzień
        var timeOffs = await ctx.GroomerTimeOffs
            .Where(t => t.SalonId == salonId)
            .Where(t => groomerIds.Contains(t.GroomerId))
            .Where(t => t.StartDate <= date && t.EndDate >= date)
            .Select(t => new { t.GroomerId, t.StartTime, t.EndTime })
            .ToListAsync(ct);

        // 8. składanie wyniku — dalej już w pamięci, bez zapytań
        var result = new List<GetAvailabilityDto>();

        foreach (var groomer in groomers)
        {
            var dto = new GetAvailabilityDto
            {
                GroomerId = groomer.Id,
                GroomerFullName = groomer.FullName,
                Date = date,
                ServiceDurationMinutes = duration
            };

            // przedziały zajęte, w minutach od północy
            var busy = visits
                .Where(v => v.GroomerId == groomer.Id)
                .Select(v =>
                {
                    var start = v.Date.Hour * 60 + v.Date.Minute;
                    return (Start: start, End: start + v.EstimatedDuration);
                })
                .Concat(timeOffs
                    .Where(t => t.GroomerId == groomer.Id)
                    .Select(t => (
                        Start: ToMinutes(t.StartTime),
                        End: ToMinutes(t.EndTime))))
                .ToList();

            foreach (var block in schedules.Where(s => s.GroomerId == groomer.Id))
            {
                var blockStart = ToMinutes(block.StartTime);
                var blockEnd = ToMinutes(block.EndTime);

                // wizyta musi zmieścić się w CAŁOŚCI wewnątrz jednego bloku pracy
                for (var slot = blockStart; slot + duration <= blockEnd; slot += SlotLengthMinutes)
                {
                    var slotEnd = slot + duration;

                    // nakładanie: nowyStart < istniejącyEnd AND nowyEnd > istniejącyStart
                    var collides = busy.Any(b => slot < b.End && slotEnd > b.Start);
                    if (collides) continue;

                    var slotDateTime = dayStart.AddMinutes(slot);
                    if (slotDateTime < minBookingTime || slotDateTime > maxBookingTime) continue;

                    dto.AvailableSlots.Add(TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(slot)).ToString("HH:mm"));
                }
            }

            dto.AvailableSlots.Sort();
            result.Add(dto);
        }

        return result;
    }

    private static int ToMinutes(TimeOnly time) => time.Hour * 60 + time.Minute;
}