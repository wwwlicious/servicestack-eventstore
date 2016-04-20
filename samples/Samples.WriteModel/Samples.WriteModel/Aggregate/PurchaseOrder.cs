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

        public PurchaseOrder AddLineItems(List<OrderLineItem> orderLineItems)
        {
            if (orderLineItems.Count > 0)
                Causes(new OrderLineItemsAdded(orderLineItems));
            return this;
        }

        public PurchaseOrder UpdateStatus(string newStatus)
        {
            if (!string.IsNullOrEmpty(newStatus))
                Causes(new OrderStatusUpdated(newStatus));
            return this;
        }
    }
}
