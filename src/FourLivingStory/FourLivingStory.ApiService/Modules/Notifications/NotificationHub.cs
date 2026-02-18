using System.Collections.Concurrent;
using System.Threading.Channels;

namespace FourLivingStory.ApiService.Modules.Notifications;

// Singleton: holds SSE channels for all connected users.
public sealed class NotificationHub
{
    private readonly ConcurrentDictionary<string, Channel<SseMessage>> _channels = new();

    public ChannelReader<SseMessage> Subscribe(string userId)
    {
        var channel = _channels.GetOrAdd(userId, _ => Channel.CreateUnbounded<SseMessage>(
            new UnboundedChannelOptions { SingleReader = true }));
        return channel.Reader;
    }

    public void Unsubscribe(string userId) => _channels.TryRemove(userId, out _);

    public void Push(string userId, SseMessage message)
    {
        if (_channels.TryGetValue(userId, out var channel))
            channel.Writer.TryWrite(message);
    }
}
