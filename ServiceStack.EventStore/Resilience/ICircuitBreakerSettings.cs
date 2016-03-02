namespace ServiceStack.EventStore.Resilience
{
    using System;

    public interface ICircuitBreakerSettings
    {
        int BreakOnNumberOfExceptions { get; }
        TimeSpan BreakCircuitForSeconds { get; }
    }
}