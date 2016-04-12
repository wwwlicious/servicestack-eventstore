namespace Samples.WriteModel.Commands
{
    using System;

    public class UpdateOrderStatus
    {
        public Guid OrderId { get; set; }
        public string NewStatus { get; set; }
    }
}
