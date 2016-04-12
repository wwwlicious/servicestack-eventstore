namespace Samples.WriteModel.Commands
{
    using System;
    using System.Collections.Generic;
    using Types;

    public class CreatePurchaseOrder
    {
        public Guid CustomerId { get; set; }

        public List<OrderLineItem> OrderLineItems { get; set; }
    }
}
