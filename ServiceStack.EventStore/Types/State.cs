namespace ServiceStack.EventStore.Types
{
    public abstract class State : IState
    {
        readonly IStateMutator mutator;

        protected State()
        {
            mutator = StateMutator.For(GetType());
            Version = 0;
        }

        public int Version { get; private set; }

        public void Apply(IDomainEvent @event)
        {
            mutator.Mutate(this, @event);
            Version++;
        }
    }
}
