namespace FourLivingStory.Application.Infrastructure.EventBus;

public interface IEventBus
{
    ValueTask PublishAsync<TEvent>(TEvent @event) where TEvent : class;
}
