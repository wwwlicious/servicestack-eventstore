using System;

namespace Samples.WriteModel.Events
{
    public class PurchaseOrderCreated
    {
        public Guid Id { get; }

        public PurchaseOrderCreated(Guid id)
        {
            Id = id;
        }
    }
}
