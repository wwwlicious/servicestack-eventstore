using System;
using System.Collections.Generic;
using Samples.AWriteModel.ServiceModel.Aggregate;
using ServiceStack.EventStore.Types;

namespace Samples.AWriteModel.ServiceModel.Events
{
    public class OrderLineItemsAdded : IAggregateEvent
    {
        public Guid OrderId { get; set; }

        public List<OrderLineItem> OrderLineItems { get; set; } 

        public OrderLineItemsAdded(List<OrderLineItem> orderLineItems)
        {
            OrderLineItems = orderLineItems;
        }
    }
}