using Grooming_Management_App.Services.SubscriptionServ;

namespace Grooming_Management_App.BackgroundServices;

public class SubscriptionScheduler(IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();

            try
            {
                var pastDue = await subscriptionService.MarkExpiredSubscriptionsAsPastDueAsync(stoppingToken);
                var suspended = await subscriptionService.SuspendExpiredSubscriptionsAsync(stoppingToken);

                Console.WriteLine($"[SubscriptionScheduler] PastDue: {pastDue}, Suspended: {suspended}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubscriptionScheduler] Failed: {ex.Message}");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}