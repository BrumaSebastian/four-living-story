using FourLivingStory.Application;
using FourLivingStory.Application.Infrastructure.EventBus;
using FourLivingStory.Infrastructure.EventBus;

namespace FourLivingStory.Infrastructure;

public sealed class InfrastructureServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
        => services.AddScoped<IEventBus, WolverineEventBus>();
}
