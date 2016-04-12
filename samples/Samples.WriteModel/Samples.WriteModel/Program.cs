using System;
using System.Collections.Generic;
using Samples.WriteModel.Commands;
using Samples.WriteModel.Events;
using Samples.WriteModel.Services.Samples.AWriteModel.ServiceInterface;
using Samples.WriteModel.Types;
using ServiceStack;
using ServiceStack.Messaging;

namespace Samples.WriteModel
{
    using ServiceStack.EventStore.Repository;

    internal class Program
    {
        static void Main(string[] args)
        {
            SetUpHost();
            ImitateClient();
            Console.ReadKey();
        }

        private static void ImitateClient()
        {
            var customerId = Guid.NewGuid();
            //Simulate an external client sending a command to create a purchase order
            ServiceStackHost.Instance.ExecuteService(new CreatePurchaseOrder
            {
                CustomerId = customerId,
                OrderLineItems = GetOrderLineItems()
            });

            //Simulate an external client sending a command to update the order status
            ServiceStackHost.Instance.ExecuteService(new UpdateOrderStatus {NewStatus = "Cancelled"});
        }

        private static List<OrderLineItem> GetOrderLineItems() => 
            new List<OrderLineItem>
            {
                new OrderLineItem {ProductId = Guid.NewGuid()},
                new OrderLineItem {ProductId = Guid.NewGuid()}
            };

        private static void SetUpHost()
        {
            new TestAppHost("Samples.WriteModel", typeof (PurchaseOrderService).Assembly)
                .Init();
        }
    }
}
