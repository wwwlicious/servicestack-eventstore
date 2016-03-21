namespace ServiceStack.EventStore.Types
{
    public interface IState
    {
        int Version { get; }
        void Apply(IAggregateEvent @event);
    }
}
