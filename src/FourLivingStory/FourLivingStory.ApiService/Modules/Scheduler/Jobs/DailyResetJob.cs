using FourLivingStory.ApiService.Infrastructure.EventBus;
using FourLivingStory.ApiService.Modules.Scheduler.Events;

namespace FourLivingStory.ApiService.Modules.Scheduler.Jobs;

public sealed class DailyResetJob(IServiceScopeFactory scopeFactory, ILogger<DailyResetJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Daily reset job started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeUntilNextMidnightUtc();
            logger.LogInformation("Next daily reset in {Delay}", delay);

            await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                break;

            var date = DateOnly.FromDateTime(DateTime.UtcNow);
            logger.LogInformation("Triggering daily reset for {Date}", date);

            // Create a scope so event handlers can access scoped services (e.g. DbContext).
            using var scope = scopeFactory.CreateScope();
            var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
            eventBus.Publish(new DailyResetTriggeredEvent(date));
        }
    }

    private static TimeSpan TimeUntilNextMidnightUtc()
    {
        var now = DateTime.UtcNow;
        var nextMidnight = now.Date.AddDays(1);
        return nextMidnight - now;
    }
}
