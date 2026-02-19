using FourLivingStory.Application.Infrastructure.EventBus;
using Wolverine;

namespace FourLivingStory.Infrastructure.EventBus;

public sealed class WolverineEventBus ( IMessageBus bus ) : IEventBus
{
	public ValueTask PublishAsync<TEvent> ( TEvent @event ) where TEvent : class
		=> bus.PublishAsync(@event);
}
