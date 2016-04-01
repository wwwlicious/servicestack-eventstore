// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 

namespace ServiceStack.EventStore.Resilience
{
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// Class that represents the retry policy for dealing with a dropped subscription.
    /// </summary>
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
