using System;
using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    public class ServiceHasReachedWarningState : Event
    {
        public DateTime WarningReached { get; }

        public ServiceHasReachedWarningState(DateTime warningReached): base("healthmessages")
        {
            WarningReached = warningReached;
        }
    }
}