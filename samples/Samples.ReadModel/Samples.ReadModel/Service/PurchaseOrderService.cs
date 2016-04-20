namespace Samples.ReadModel.Service
{
    using System;
    using Events;
    using ServiceStack.EventStore.Factories;
    using ServiceStack.EventStore.Projections;
    using Types;

    //To consume events via any type of subscription we 
    //create a class that inherits from ServiceStack.Service
    //and add methods that take the CLR type of the event 
    //as a parameter
    public class PurchaseOrderService : ServiceStack.Service
    {
        //to use Redis for our read model we get an instance of 
        //a RedisReadModelWriter by providing the CLR type of the 
        //unique id and a class that represents a record in the read model
        private readonly IReadModelWriter<Guid, OrderViewModel> writer =
                        ReadModelWriterFactory.GetRedisWriter<Guid, OrderViewModel>();

        //we decide which event type necessitates the creation of 
        //a new record in the read model and pass in a new instance 
        //of the view model to represent that record with a unique Id
        //in this case the OrderId is being used
        public object Any(PurchaseOrderCreated @event) => writer.Add(new OrderViewModel(@event.Id));

        //we update the read model by passing a delegate
        //which updates the properties of our view model proxy
        public object Any(OrderLineItemsAdded @event) => writer.Update(@event.OrderId,
            vm => vm.LineItemCount += @event.OrderLineItems.Count);

        public object Any(OrderStatusUpdated @event) => writer.Update(@event.OrderId,
            vm => vm.OrderStatus = @event.NewStatus);
    }
}
