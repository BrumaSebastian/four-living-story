namespace FourLivingStory.ApiService.Infrastructure.EventBus;

public interface IEventBus
{
    void Publish<TEvent>(TEvent @event) where TEvent : class;
}
