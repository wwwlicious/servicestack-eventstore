using System.Collections.Generic;
using Samples.WriteModel.Events;
using Samples.WriteModel.Types;
using ServiceStack.EventStore.StateManagement;

namespace Samples.WriteModel.Aggregate
{
    public class PurchaseOrderState : State
    {
        public string OrderStatus { get; private set; }
        public List<OrderLineItem> OrderLineItems { get; } = new List<OrderLineItem>();

        public void On(PurchaseOrderCreated @event) { }

        public void On(OrderLineItemsAdded @event) => OrderLineItems.AddRange(@event.OrderLineItems);

        public void On(OrderStatusUpdated @event) => OrderStatus = @event.NewStatus;
    }
}
