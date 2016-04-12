using System;
using System.Collections.Generic;
using Samples.WriteModel.Events;
using Samples.WriteModel.Types;
using ServiceStack.EventStore.Types;

namespace Samples.WriteModel.Aggregate
{
    public class PurchaseOrder : Aggregate<PurchaseOrderState>
    {
        public PurchaseOrder(Guid id) : base(id) { }

        public PurchaseOrder() : base(Guid.NewGuid())
        {
            Causes(new PurchaseOrderCreated(Id));
        }

        public void AddLineItems(List<OrderLineItem> orderLineItems)
        {
            if (orderLineItems.Count > 0)
                Causes(new OrderLineItemsAdded(orderLineItems));
        }

        public void UpdateStatus(string newStatus)
        {
            if (!string.IsNullOrEmpty(newStatus))
                Causes(new OrderStatusUpdated(newStatus));
        }
    }
}
