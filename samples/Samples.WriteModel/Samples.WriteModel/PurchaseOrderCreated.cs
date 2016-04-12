namespace Samples.WriteModel
{
    using System;

    public class PurchaseOrderCreated
    {
        public Guid Id { get; }

        public PurchaseOrderCreated(Guid id)
        {
            Id = id;
        }
    }
}
