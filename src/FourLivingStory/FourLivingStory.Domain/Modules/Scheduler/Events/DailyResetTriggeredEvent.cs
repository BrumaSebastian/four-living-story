namespace FourLivingStory.Domain.Modules.Scheduler.Events;

public sealed record DailyResetTriggeredEvent(DateOnly Date);
