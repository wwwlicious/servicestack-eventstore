namespace ServiceStack.EventStore.Types
{
    /// <summary>
    /// Holds the state of an aggregate in memory and will be used as the snapshot for that aggregate.
    /// </summary>
    public abstract class State : IState
    {
        private readonly IStateMutator mutator;
        private const int initialVersion = 0;

        protected State()
        {
            mutator = StateMutator.For(GetType());
            Version = initialVersion;
        }

        public int Version { get; private set; }

        public void Apply(IDomainEvent @event)
        {
            mutator.Mutate(this, @event);
            Version++;
        }
    }
}
