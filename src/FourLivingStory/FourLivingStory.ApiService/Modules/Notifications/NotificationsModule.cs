using FourLivingStory.ApiService.Modules.Identity;

namespace FourLivingStory.ApiService.Modules.Notifications;

public static class NotificationsModule
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        services.AddSingleton<NotificationHub>();
        return services;
    }

    public static IEndpointRouteBuilder MapNotificationsModule(this IEndpointRouteBuilder app)
    {
        app.MapGet("/notifications/stream", async (
            ICurrentUser currentUser,
            NotificationHub hub,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            if (!currentUser.IsAuthenticated)
                return Results.Unauthorized();

            httpContext.Response.Headers.ContentType  = "text/event-stream";
            httpContext.Response.Headers.CacheControl = "no-cache";
            httpContext.Response.Headers.Connection   = "keep-alive";

            var reader = hub.Subscribe(currentUser.UserId);
            try
            {
                await foreach (var message in reader.ReadAllAsync(ct))
                {
                    await httpContext.Response.WriteAsync($"event: {message.Event}\n", ct);
                    await httpContext.Response.WriteAsync($"data: {message.Data}\n\n", ct);
                    await httpContext.Response.Body.FlushAsync(ct);
                }
            }
            finally
            {
                hub.Unsubscribe(currentUser.UserId);
            }

            return Results.Empty;
        })
        .WithName("GetNotificationStream")
        .RequireAuthorization();

        return app;
    }
}
