using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    public class DestinationChanged : IDomainEvent
    {
        public string Destination { get; }

        public DestinationChanged(string destination)
        {
            Destination = destination;
        }
    }
}