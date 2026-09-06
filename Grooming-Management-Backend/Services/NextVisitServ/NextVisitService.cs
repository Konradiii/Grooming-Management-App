using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.NextVisitDTO;
using Grooming_Management_App.Enums;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.NextVisitServ;

public class NextVisitService(GroomingDbContext ctx) : INextVisitReaderService
{
    
    private static readonly TimeZoneInfo PolishTime =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");
    
    private const int MinVisitsForPrediction = 3;
    
    private const int MinToleranceDays = 7;

    private const int HistoryWindowDays = 730;

    private const int MaxDaysSinceLastVisit = 365;


    public async Task<List<GetDueDogDto>> GetDueDogsAsync(int salonId, CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;
        var todayLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, PolishTime));
        var historyCutoffUtc = nowUtc.AddDays(-HistoryWindowDays);

        var dogsWithUpcomingVisit = await ctx.Visits
            .Where(v => v.SalonId == salonId)
            .Where(v => v.Status == StatusEnum.Scheduled)
            .Where(v => v.Date >= nowUtc)
            .Select(v => v.DogId)
            .Distinct()
            .ToListAsync(ct);
        
        var blacklistedOwnerIds = await ctx.Blacklists
            .Where(b => b.SalonId == salonId)
            .Select(b => b.DogOwnerId)
            .Distinct()
            .ToListAsync(ct);
        
        var visits = await ctx.Visits
            .Where(v => v.SalonId == salonId)
            .Where(v => v.Status == StatusEnum.Completed)
            .Where(v => v.Date >= historyCutoffUtc)
            .Where(v => !dogsWithUpcomingVisit.Contains(v.DogId))
            .Where(v => !blacklistedOwnerIds.Contains(v.DogOwnerId))
            .Select(v => new VisitRow
            {
                DogId = v.DogId,
                DateUtc = v.Date,
                DogName = v.Dog.Name,
                BreedName = v.Dog.Breed.Name,
                DogOwnerId = v.DogOwnerId,
                DogOwnerFirstName = v.DogOwner.FirstName,
                DogOwnerLastName = v.DogOwner.LastName,
                DogOwnerPhone = v.DogOwner.Phone
            })
            .ToListAsync(ct);
        
        var result = new List<GetDueDogDto>();
        
         foreach (var group in visits.GroupBy(v => v.DogId))
        {
            if (group.Count() < MinVisitsForPrediction)
            {
                continue;
            }

            var dates = group
                .Select(v => DateOnly.FromDateTime(
                    TimeZoneInfo.ConvertTimeFromUtc(v.DateUtc, PolishTime)))
                .OrderBy(d => d)
                .ToList();

            var interval = CalculateInterval(dates);

            if (interval == null)
            {
                continue;
            }

            var lastVisit = dates[^1];

            if (todayLocal.DayNumber - lastVisit.DayNumber > MaxDaysSinceLastVisit)
            {
                continue;
            }

            var predictedDate = lastVisit.AddDays(interval.Value.TypicalDays);
            var windowEnd = predictedDate.AddDays(interval.Value.ToleranceDays);

            // Spóźniony dopiero po całym oknie, nie po samej dacie środkowej.
            if (todayLocal <= windowEnd)
            {
                continue;
            }

            var first = group.First();

            result.Add(new GetDueDogDto
            {
                DogId = group.Key,
                DogName = first.DogName,
                BreedName = first.BreedName,
                DogOwnerId = first.DogOwnerId,
                DogOwnerFullName = $"{first.DogOwnerFirstName} {first.DogOwnerLastName}",
                DogOwnerPhone = first.DogOwnerPhone,
                LastVisitDate = lastVisit,
                PredictedDate = predictedDate,
                TypicalIntervalDays = interval.Value.TypicalDays,
                ToleranceDays = interval.Value.ToleranceDays,
                DaysOverdue = todayLocal.DayNumber - predictedDate.DayNumber,
                VisitsCount = dates.Count
            });
        }

        return result.OrderByDescending(d => d.DaysOverdue).ToList();
    }

    // Mediana, nie średnia — jeden wyjazd właściciela nie może wypchnąć predykcji o miesiąc.
    private static (int TypicalDays, int ToleranceDays)? CalculateInterval(List<DateOnly> sortedDates)
    {
        if (sortedDates.Count < MinVisitsForPrediction)
        {
            return null;
        }

        var gaps = new List<int>();

        for (var i = 1; i < sortedDates.Count; i++)
        {
            gaps.Add(sortedDates[i].DayNumber - sortedDates[i - 1].DayNumber);
        }

        gaps.Sort();

        var middle = gaps.Count / 2;

        var median = gaps.Count % 2 == 1
            ? gaps[middle]
            : (int)Math.Round((gaps[middle - 1] + gaps[middle]) / 2.0);

        if (median <= 0)
        {
            return null;
        }

        var spread = (gaps[^1] - gaps[0]) / 2;
        var tolerance = Math.Max(spread, MinToleranceDays);

        return (median, tolerance);
    }

    private class VisitRow
    {
        public int DogId { get; set; }
        public DateTime DateUtc { get; set; }
        public string DogName { get; set; } = string.Empty;
        public string BreedName { get; set; } = string.Empty;
        public int DogOwnerId { get; set; }
        public string DogOwnerFirstName { get; set; } = string.Empty;
        public string DogOwnerLastName { get; set; } = string.Empty;
        public string DogOwnerPhone { get; set; } = string.Empty;
    }
}
