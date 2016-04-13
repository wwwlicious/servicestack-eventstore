namespace Sample.AReadModel.ServiceModel.Types
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
