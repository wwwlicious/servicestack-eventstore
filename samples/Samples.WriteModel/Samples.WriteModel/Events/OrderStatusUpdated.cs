using System;

namespace Samples.WriteModel.Events
{
    public class OrderStatusUpdated
    {
        public Guid OrderId { get; set; }
        public string NewStatus { get; }

        public OrderStatusUpdated(string newStatus)
        {
            NewStatus = newStatus;
        }
    }
}
