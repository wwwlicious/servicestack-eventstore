using System;

namespace ServiceStack.AWriteModel.ServiceModel.Commands
{
    public class UpdateOrderStatus
    {
        public Guid OrderId { get; set; }
        public string NewStatus { get; set; }
    }
}