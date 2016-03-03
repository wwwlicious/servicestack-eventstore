namespace ServiceStack.EventStore.Resilience
{
    using System;
    using Polly;
    using Polly.CircuitBreaker;
    using Logging;

    public class CircuitBreaker : ICircuitBreaker
    {
        private CircuitBreakerPolicy policy;
        private readonly ILog log;

        public CircuitBreaker()
        {
            log = LogManager.GetLogger(GetType());
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
                log.Error(e);
            }
        }

        private void OnHalfOpen()
        {
            log.Info("OnHalfOpen");
        }

        private void OnReset()
        {
            log.Info("OnReset");
        }

        private void OnBreak(Exception arg1, TimeSpan arg2)
        {
            log.Info("OnBreak");
        }
    }
}
