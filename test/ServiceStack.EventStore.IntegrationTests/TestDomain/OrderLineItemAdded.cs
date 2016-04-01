using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    public class OrderLineItemAdded : IAggregateEvent
    {
        public OrderLineItem NewItem { get; }

        public OrderLineItemAdded(OrderLineItem newItem)
        {
            NewItem = newItem;
        }
    }
}