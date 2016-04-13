namespace Samples.ReadModel.Events
{
    using System;

    public class PurchaseOrderCreated 
    {
        public Guid Id { get; set;  }

        public PurchaseOrderCreated(Guid id)
        {
            Id = id;
        }
    }
}
