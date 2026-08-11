using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Services.NotificationServ;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.BackgroundServices;

public class ReminderScheduler(IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(30);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var windowStart = DateTime.UtcNow.AddHours(24);
            var windowEnd = windowStart.Add(_interval);
            
            using var scope = scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<GroomingDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var correctVisits =  await ctx.Visits
                .IgnoreQueryFilters()
                .Where(e => windowStart < e.Date && e.Date <= windowEnd)
                .Where(e=> e.Status == StatusEnum.Scheduled)
                .ToListAsync(stoppingToken);

            foreach (var visit in correctVisits)
            {
                try
                {
                    Console.WriteLine($"[ReminderScheduler] Processing visit {visit.Id}");
                    Console.WriteLine($"[ReminderScheduler] SalonId: {visit.SalonId}");
                    Console.WriteLine($"[ReminderScheduler] NotificationService: {notificationService != null}");

                    await notificationService.SendVisitReminderAsync(
                        visit.SalonId,
                        visit.Id,
                        stoppingToken);

                    Console.WriteLine($"[ReminderScheduler] Finished visit {visit.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"[ReminderScheduler] Failed to send reminder for visit {visit.Id}: {ex}");
                }
            }
            await Task.Delay(_interval, stoppingToken);
            
        }
    }
}