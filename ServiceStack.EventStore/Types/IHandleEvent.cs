namespace ServiceStack.EventStore.Types
{
    public interface IHandleEvent<in TEvent>
    {
        void Handle(TEvent @event);
    }
}
