namespace FourLivingStory.Application.Modules.Identity;

public sealed class IdentityServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
    }
}
