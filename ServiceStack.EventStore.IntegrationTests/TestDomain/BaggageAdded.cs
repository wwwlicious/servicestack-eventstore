using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    public class BaggageAdded : IAggregateEvent
    {
        public int NoOfBags { get; }

        public BaggageAdded(int noOfBags)
        {
            NoOfBags = noOfBags;
        }
    }
}