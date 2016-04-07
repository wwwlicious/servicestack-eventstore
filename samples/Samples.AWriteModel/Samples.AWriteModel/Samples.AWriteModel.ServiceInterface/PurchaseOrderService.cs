using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Samples.AWriteModel.ServiceModel.Aggregate;
using ServiceStack;
using ServiceStack.AWriteModel.ServiceModel.Commands;
using ServiceStack.EventStore.Repository;

namespace Samples.AWriteModel.ServiceInterface
{
    public class PurchaseOrderService : Service
    {
        private readonly IEventStoreRepository eventStore;

        public PurchaseOrderService(IEventStoreRepository eventStore)
        {
            this.eventStore = eventStore;
        }

        public async Task<object> Any(CreatePurchaseOrder cmd)
        {
            var order = new PurchaseOrder();

            order.AddLineItems(cmd.OrderLineItems);

            await eventStore.SaveAsync(order);

            return order.Id;
        }

        public async Task<object> Any(UpdateOrderStatus cmd)
        {
            var order = await eventStore.GetByIdAsync<PurchaseOrder>(cmd.OrderId);

            order.UpdateStatus(cmd.NewStatus);

            await eventStore.SaveAsync(order);

            return Task.FromResult(false);
        }
    }
}