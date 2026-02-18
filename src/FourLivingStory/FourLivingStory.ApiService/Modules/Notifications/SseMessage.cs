namespace FourLivingStory.ApiService.Modules.Notifications;

public sealed record SseMessage(string Event, string Data);
