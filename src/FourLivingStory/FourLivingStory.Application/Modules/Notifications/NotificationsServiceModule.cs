namespace FourLivingStory.Application.Modules.Notifications;

public sealed class NotificationsServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
        => services.AddSingleton<NotificationHub>();
}
