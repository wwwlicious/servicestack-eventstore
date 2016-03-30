namespace ServiceStack.EventStore.StateManagement
{
    using Types;

    /// <summary>
    /// Interface that represents the state of an aggregate
    /// Original source: https://github.com/mfelicio/NDomain/blob/d30322bc64105ad2e4c961600ae24831f675b0e9/source/NDomain/IState.cs
    /// </summary>
    public interface IState
    {
        int Version { get; }
        void Apply(IAggregateEvent @event);
    }
}
