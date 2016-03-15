namespace ServiceStack.EventStore.Types
{
    public interface IState
    {
        int Version { get; }
        void Apply(IDomainEvent @event);
    }
}
