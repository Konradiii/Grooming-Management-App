using Grooming_Management_App.DataInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.BackgroundServices;

public class TokenCleanupScheduler(
    IServiceScopeFactory scopeFactory,
    ILogger<TokenCleanupScheduler> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var ctx = scope.ServiceProvider.GetRequiredService<GroomingDbContext>();

                var cutoff = DateTime.UtcNow.AddDays(-30);

                var deleted = await ctx.RefreshTokens
                    .Where(t => t.ExpiresAt < cutoff)
                    .ExecuteDeleteAsync(stoppingToken);

                if (deleted > 0)
                {
                    logger.LogInformation("Deleted {Count} expired refresh tokens", deleted);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Token cleanup failed");
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