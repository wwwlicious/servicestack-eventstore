﻿namespace Samples.ReadModel.Events
{
    using System;
    using Types;
    using System.Collections.Generic;

    public class OrderLineItemsAdded {

        public Guid OrderId { get; set; }

        public List<OrderLineItem> OrderLineItems { get; set; } 

        public OrderLineItemsAdded(List<OrderLineItem> orderLineItems)
        {
            OrderLineItems = orderLineItems;
        }
    }
}
