using System;
using System.Collections.Generic;
using Samples.AWriteModel.ServiceModel.Aggregate;

namespace ServiceStack.AWriteModel.ServiceModel.Commands
{
    public class CreatePurchaseOrder
    {
        public Guid CustomerId { get; set; }

        public List<OrderLineItem> OrderLineItems { get; set; }
    }
}