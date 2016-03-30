namespace ServiceStack.EventStore.Dispatcher
{
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;

    internal interface IEventDispatcher
    {
        Task<bool> Dispatch(ResolvedEvent @event);
    }
}