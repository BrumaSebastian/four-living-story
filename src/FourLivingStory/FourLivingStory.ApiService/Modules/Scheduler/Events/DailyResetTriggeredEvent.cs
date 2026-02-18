namespace FourLivingStory.ApiService.Modules.Scheduler.Events;

public sealed record DailyResetTriggeredEvent(DateOnly Date);
