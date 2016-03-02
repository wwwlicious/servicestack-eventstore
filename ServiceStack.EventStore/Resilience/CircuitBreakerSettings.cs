namespace ServiceStack.EventStore.Resilience
{
    using System;

    public class CircuitBreakerSettings : ICircuitBreakerSettings
    {
        public CircuitBreakerSettings() : this(3, TimeSpan.FromSeconds(20)) {}

        public CircuitBreakerSettings(int breakOnNumberOfExceptions, TimeSpan breakCircuitForSeconds)
        {
            BreakOnNumberOfExceptions = breakOnNumberOfExceptions;
            BreakCircuitForSeconds = breakCircuitForSeconds;
        }

        public int BreakOnNumberOfExceptions { get; }
        public TimeSpan BreakCircuitForSeconds { get; }
    }
}