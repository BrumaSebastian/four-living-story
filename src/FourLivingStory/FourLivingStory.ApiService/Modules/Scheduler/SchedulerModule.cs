using FourLivingStory.ApiService.Modules.Scheduler.Jobs;

namespace FourLivingStory.ApiService.Modules.Scheduler;

public static class SchedulerModule
{
    public static IServiceCollection AddSchedulerModule(this IServiceCollection services)
    {
        services.AddHostedService<DailyResetJob>();
        return services;
    }
}
