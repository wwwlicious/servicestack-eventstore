using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    public class DestinationChanged : IAggregateEvent
    {
        public string Destination { get; set;  }

        public DestinationChanged(string destination)
        {
            Destination = destination;
        }
    }
}