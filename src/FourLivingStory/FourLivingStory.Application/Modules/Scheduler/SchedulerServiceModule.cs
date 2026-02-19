using FourLivingStory.Application.Modules.Scheduler.Jobs;

namespace FourLivingStory.Application.Modules.Scheduler;

public sealed class SchedulerServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddHostedService<DailyResetJob>();
    }
}
