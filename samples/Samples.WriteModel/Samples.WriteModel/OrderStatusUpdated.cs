namespace Samples.WriteModel
{
    using System;

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
