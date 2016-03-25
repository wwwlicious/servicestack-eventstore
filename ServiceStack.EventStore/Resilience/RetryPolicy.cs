namespace ServiceStack.EventStore.Resilience
{
    using System.Collections.Generic;
    using System;

    public class RetryPolicy
    {
        public RetryPolicy(IEnumerable<TimeSpan> sleepDurations)
        {
            SleepDurations = sleepDurations;
            RetryType = RetryType.Durations;
        }

        public RetryType RetryType { get; }

        public RetryPolicy(Retries maxNoOfRetries, Func<int, TimeSpan> sleepDurationProvider)
        {
            MaxNoOfRetries = maxNoOfRetries;
            SleepDurationProvider = sleepDurationProvider;
            RetryType = RetryType.Provider;
        }

        public Retries MaxNoOfRetries { get; }

        public Func<int, TimeSpan> SleepDurationProvider { get; }

        public IEnumerable<TimeSpan> SleepDurations { get; }
    }
}
