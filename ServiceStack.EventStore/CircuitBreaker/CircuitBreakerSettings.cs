namespace ServiceStack.EventStore.CircuitBreaker
{
    using System;

    public class CircuitBreakerSettings : ICircuitBreakerSettings
    {
        public CircuitBreakerSettings(int breakOnNumberOfExceptions, TimeSpan breakCircuitForSeconds)
        {
            BreakOnNumberOfExceptions = breakOnNumberOfExceptions;
            BreakCircuitForSeconds = breakCircuitForSeconds;
        }

        public int BreakOnNumberOfExceptions { get; }
        public TimeSpan BreakCircuitForSeconds { get; }
    }
}