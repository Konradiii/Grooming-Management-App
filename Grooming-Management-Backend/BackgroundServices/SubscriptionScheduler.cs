using Grooming_Management_App.Services.SubscriptionServ;

namespace Grooming_Management_App.BackgroundServices;

public class SubscriptionScheduler(
    IServiceScopeFactory scopeFactory,
    ILogger<SubscriptionScheduler> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();

                var pastDue = await subscriptionService.MarkExpiredSubscriptionsAsPastDueAsync(stoppingToken);
                var suspended = await subscriptionService.SuspendExpiredSubscriptionsAsync(stoppingToken);
                var smsReset = await subscriptionService.ResetMonthlySmsPackagesAsync(stoppingToken);

                if (pastDue > 0 || suspended > 0)
                {
                    logger.LogInformation(
                        "Subscription check: {PastDueCount} marked past due, {SuspendedCount} suspended",
                        pastDue, suspended);
                }

                if (smsReset > 0)
                {
                    logger.LogInformation("Reset SMS package for {Count} salons", smsReset);
                }
                
                if (pastDue > 0 || suspended > 0)
                {
                    logger.LogInformation(
                        "Subscription check: {PastDueCount} marked past due, {SuspendedCount} suspended",
                        pastDue, suspended);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Subscription check failed");
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