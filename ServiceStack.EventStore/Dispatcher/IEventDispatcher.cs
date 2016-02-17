using EventStore.ClientAPI;

namespace ServiceStack.EventStore.Dispatcher
{
    public interface IEventDispatcher
    {
        bool Dispatch(ResolvedEvent @event);
    }
}