namespace ServiceStack.EventStore.Types
{
    public interface IStateMutator
    {
        void Mutate(IState state, IDomainEvent @event);
    }
}
