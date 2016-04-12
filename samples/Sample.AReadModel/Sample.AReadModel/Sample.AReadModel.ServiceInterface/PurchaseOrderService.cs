namespace Sample.AReadModel.ServiceInterface
{
    using System;
    using ServiceStack;
    using ServiceModel.Types;
    using ServiceStack.EventStore.Factories;
    using ServiceStack.EventStore.Projections;

    public class PurchaseOrderService : Service
    {
        private readonly IReadModelWriter<Guid, OrderViewModel> writer = 
                        ReadModelWriterFactory.GetRedisWriter<Guid, OrderViewModel>();

        public object Any(PurchaseOrderCreated @event)
        {
            return writer.Add(new OrderViewModel(@event.Id));
        }

        public object Any(OrderLineItemsAdded @event)
        {
            return writer.Update(@event.OrderId, 
                    vm => vm.LineItemCount += @event.OrderLineItems.Count);
        }

        public object Any(OrderStatusUpdated @event)
        {
            return writer.Update(@event.OrderId,
                vm => vm.OrderStatus = @event.NewStatus);
        }
    }
}