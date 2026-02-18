namespace FourLivingStory.ApiService.Modules.Rewards;

public static class RewardsModule
{
    public static IServiceCollection AddRewardsModule(this IServiceCollection services)
    {
        services.AddSingleton<RewardCalculator>();
        return services;
    }
}
