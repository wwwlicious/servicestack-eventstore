namespace ServiceStack.EventStore.Types
{
    public interface IHandle<in TEvent>
    {
        void Handle(TEvent @event);
    }
}
