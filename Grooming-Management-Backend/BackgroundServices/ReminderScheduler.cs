using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
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
                using var scope = scopeFactory.CreateScope();
                var ctx = scope.ServiceProvider.GetRequiredService<GroomingDbContext>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var salons = await ctx.Salons
                    .Where(s => s.RemindersEnabled)
                    .Where(s => s.SubscriptionStatus != SubscriptionStatusEnum.Suspended)
                    .Select(s => new { s.Id, s.ReminderHoursBefore })
                    .ToListAsync(stoppingToken);

                foreach (var salon in salons)
                {
                    var windowStart = DateTime.UtcNow.AddHours(salon.ReminderHoursBefore);
                    var windowEnd = windowStart.Add(_interval);

                    var visits = await ctx.Visits
                        .IgnoreQueryFilters()
                        .Where(v => v.SalonId == salon.Id)
                        .Where(v => windowStart < v.Date && v.Date <= windowEnd)
                        .Where(v => v.Status == StatusEnum.Scheduled)
                        .ToListAsync(stoppingToken);

                    if (visits.Count == 0) continue;

                    logger.LogInformation(
                        "Found {Count} visits to remind in salon {SalonId}, window {WindowStart} - {WindowEnd}",
                        visits.Count, salon.Id, windowStart, windowEnd);

                    foreach (var visit in visits)
                    {
                        try
                        {
                            await notificationService.SendVisitReminderAsync(
                                visit.SalonId,
                                visit.Id,
                                stoppingToken);

                            logger.LogInformation("Reminder sent for visit {VisitId}", visit.Id);
                        }
                        catch (ConflictException ex) when (ex.Message == ErrorCodes.SmsLimitExceeded)
                        {
                            logger.LogInformation(
                                "Skipping reminder for visit {VisitId} - salon {SalonId} has no SMS left",
                                visit.Id, visit.SalonId);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex,
                                "Failed to send reminder for visit {VisitId} in salon {SalonId}",
                                visit.Id, visit.SalonId);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Reminder cycle failed");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}