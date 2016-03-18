namespace ServiceStack.EventStore.Types
{
    public interface IHandleEvent {}

    public interface IHandleEvent<in TEvent>: IHandleEvent
    {
        void Handle(TEvent @event);
    }
}
