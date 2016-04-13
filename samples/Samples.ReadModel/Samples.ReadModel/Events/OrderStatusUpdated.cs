namespace Samples.ReadModel.Events
{
    using System;

    public class OrderStatusUpdated
    {
        public Guid OrderId { get; set; }

        public string NewStatus { get; set; }
    }
}
