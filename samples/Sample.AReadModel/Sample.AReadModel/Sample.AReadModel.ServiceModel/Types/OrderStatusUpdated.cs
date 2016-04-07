namespace Sample.AReadModel.ServiceModel.Types
{
    using System;

    public class OrderStatusUpdated
    {
        public Guid OrderId { get; set; }

        public string NewStatus { get; set; }
    }
}
