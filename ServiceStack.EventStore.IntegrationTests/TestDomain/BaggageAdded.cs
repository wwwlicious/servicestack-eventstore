using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    public class BaggageAdded : IDomainEvent
    {
        public int NoOfBags { get; }

        public BaggageAdded(int noOfBags)
        {
            NoOfBags = noOfBags;
        }
    }
}