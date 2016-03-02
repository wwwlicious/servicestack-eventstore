using System;

namespace ServiceStack.EventStore.Resilience
{
    public interface ICircuitBreakerSettings
    {
        int BreakOnNumberOfExceptions { get; }
        TimeSpan BreakCircuitForSeconds { get; }
    }
}