using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples.WriteModel
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
