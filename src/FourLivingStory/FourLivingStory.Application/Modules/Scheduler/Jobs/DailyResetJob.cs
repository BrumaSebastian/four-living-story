using FourLivingStory.Application.Infrastructure.EventBus;
using FourLivingStory.Domain.Modules.Scheduler.Events;

namespace FourLivingStory.Application.Modules.Scheduler.Jobs;

public sealed class DailyResetJob(
    IServiceScopeFactory scopeFactory,
    ILogger<DailyResetJob> logger,
    TimeProvider timeProvider)
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

            var date = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
            logger.LogInformation("Triggering daily reset for {Date}", date);

            // Create a scope so event handlers can access scoped services (e.g. DbContext).
            using var scope = scopeFactory.CreateScope();
            var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
            await eventBus.PublishAsync(new DailyResetTriggeredEvent(date));
        }
    }

    private TimeSpan TimeUntilNextMidnightUtc()
    {
        var now = timeProvider.GetUtcNow();
        var nextMidnight = now.UtcDateTime.Date.AddDays(1);
        return nextMidnight - now.UtcDateTime;
    }
}
