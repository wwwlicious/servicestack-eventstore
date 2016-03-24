namespace ServiceStack.EventStore.Dispatcher
{
    using global::EventStore.ClientAPI;

    public interface IEventDispatcher
    {
        bool Dispatch(ResolvedEvent @event);
    }
}