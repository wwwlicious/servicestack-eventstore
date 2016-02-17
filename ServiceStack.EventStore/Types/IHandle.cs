namespace ServiceStack.EventStore.Types
{
    public interface IHandle<in TNotification>
    {
        void Handle(TNotification @event);
    }
}
