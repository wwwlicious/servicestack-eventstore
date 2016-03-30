namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    public class OrderLineItem
    {
        public OrderLineItem(int quantity)
        {
            Quantity = quantity;
        }

        public int Quantity { get;  }
    }
}