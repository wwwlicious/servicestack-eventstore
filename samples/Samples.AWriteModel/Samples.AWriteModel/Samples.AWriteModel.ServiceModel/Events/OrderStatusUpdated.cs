using System;
using ServiceStack.EventStore.Types;

namespace Samples.AWriteModel.ServiceModel.Events
{
    public class OrderStatusUpdated : IAggregateEvent
    {
        public Guid OrderId { get; set; }
        public string NewStatus { get; }

        public OrderStatusUpdated(string newStatus)
        {
            NewStatus = newStatus;
        }
    }
}