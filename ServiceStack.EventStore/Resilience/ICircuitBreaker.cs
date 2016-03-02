namespace ServiceStack.EventStore.Resilience
{
    using System;
    using Polly.CircuitBreaker;

    public interface ICircuitBreaker
    {
        CircuitState CircuitState();
        void Execute(Action action);
        void Isolate();
        void Reset();
    }
}