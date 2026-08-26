using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Services.NotificationServ;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.BackgroundServices;

public class ReminderScheduler(IServiceScopeFactory scopeFactory, ILogger<ReminderScheduler> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(20);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var windowStart = DateTime.UtcNow.AddHours(24);
                var windowEnd = windowStart.Add(_interval);

                using var scope = scopeFactory.CreateScope();
                var ctx = scope.ServiceProvider.GetRequiredService<GroomingDbContext>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var correctVisits = await ctx.Visits
                    .IgnoreQueryFilters()
                    .Where(e => windowStart < e.Date && e.Date <= windowEnd)
                    .Where(e => e.Status == StatusEnum.Scheduled)
                    .ToListAsync(stoppingToken);

                if (correctVisits.Count > 0)
                {
                    logger.LogInformation(
                        "Found {Count} visits to remind in window {WindowStart} - {WindowEnd}",
                        correctVisits.Count, windowStart, windowEnd);
                }

                foreach (var visit in correctVisits)
                {
                    try
                    {
                        await notificationService.SendVisitReminderAsync(
                            visit.SalonId,
                            visit.Id,
                            stoppingToken);

                        logger.LogInformation("Reminder sent for visit {VisitId}", visit.Id);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex,
                            "Failed to send reminder for visit {VisitId} in salon {SalonId}",
                            visit.Id, visit.SalonId);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Reminder cycle failed");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}