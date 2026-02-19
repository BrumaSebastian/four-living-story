namespace FourLivingStory.Application.Modules.Rewards;

public sealed class RewardsServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
        => services.AddSingleton<RewardCalculator>();
}
