namespace ServiceStack.EventStore.CircuitBreaker
{
    using System;
    using Polly;
    using Polly.CircuitBreaker;

    public class CircuitBreaker
    {
        private readonly ICircuitBreakerSettings settings;
        private CircuitBreakerPolicy policy;

        public CircuitBreaker(ICircuitBreakerSettings settings)
        {
            this.settings = settings;
        }

        public CircuitState CircuitState()
        {
            return policy.CircuitState;
        }

        public void Isolate()
        {
            policy.Isolate();
        }

        public void Reset()
        {
            policy.Reset();
        }

        public void Execute(Action action)
        {
            try
            {
                Policy.Handle<Exception>()
                    .CircuitBreaker(3, TimeSpan.FromSeconds(5), OnBreak, OnReset, OnHalfOpen)
                    .Execute(action);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void OnHalfOpen()
        {
            Console.WriteLine("OnHalfOpen");
        }

        private void OnReset()
        {
            Console.WriteLine("OnReset");
        }

        private void OnBreak(Exception arg1, TimeSpan arg2)
        {
            Console.WriteLine("OnBreak");
        }
    }
}
