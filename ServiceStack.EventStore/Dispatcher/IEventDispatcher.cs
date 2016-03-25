namespace ServiceStack.EventStore.Dispatcher
{
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;

    public interface IEventDispatcher
    {
        Task<bool> Dispatch(ResolvedEvent @event);
    }
}