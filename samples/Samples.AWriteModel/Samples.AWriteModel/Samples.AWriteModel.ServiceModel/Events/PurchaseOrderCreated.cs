using System;
using ServiceStack.EventStore.Types;

namespace Samples.AWriteModel.ServiceModel.Events
{
    public class PurchaseOrderCreated : IAggregateEvent
    {
        public Guid Id { get; }

        public PurchaseOrderCreated(Guid id)
        {
            Id = id;
        }
    }
}