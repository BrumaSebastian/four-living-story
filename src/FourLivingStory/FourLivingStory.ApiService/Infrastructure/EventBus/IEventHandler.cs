namespace FourLivingStory.ApiService.Infrastructure.EventBus;

public interface IEventHandler<TEvent> where TEvent : class
{
    void Handle(TEvent @event);
}
