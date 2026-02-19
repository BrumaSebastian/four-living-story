using FourLivingStory.Application;
using FourLivingStory.Application.Modules.Identity;
using FourLivingStory.Application.Modules.Notifications;

namespace FourLivingStory.ApiService.Endpoints.Notifications;

public sealed class NotificationsEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
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
    }
}
