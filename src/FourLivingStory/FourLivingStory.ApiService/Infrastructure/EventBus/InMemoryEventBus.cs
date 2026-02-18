namespace FourLivingStory.ApiService.Infrastructure.EventBus;

// Scoped: resolves handlers from the current request's DI scope so all handlers
// share the same DbContext instance and participate in the same unit of work.
public sealed class InMemoryEventBus(IServiceProvider serviceProvider) : IEventBus
{
    public void Publish<TEvent>(TEvent @event) where TEvent : class
    {
        var handlers = serviceProvider.GetServices<IEventHandler<TEvent>>();
        foreach (var handler in handlers)
            handler.Handle(@event);
    }
}
