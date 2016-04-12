using System;
using System.Collections.Generic;
using Samples.WriteModel.Types;

namespace Samples.WriteModel.Events
{
    public class OrderLineItemsAdded
    {
        public Guid OrderId { get; set; }

        public List<OrderLineItem> OrderLineItems { get; set; }

        public OrderLineItemsAdded(List<OrderLineItem> orderLineItems)
        {
            OrderLineItems = orderLineItems;
        }
    }
}
