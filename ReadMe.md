# ServiceStack.EventStore #

[![Build status](https://ci.appveyor.com/api/projects/status/v9qd6kso0bkc5spf/branch/master?svg=true)](https://ci.appveyor.com/project/wwwlicious/servicestack-eventstore/branch/master)

### A plugin for [ServiceStack](https://servicestack.net/) that provides a [message gateway](http://www.enterpriseintegrationpatterns.com/patterns/messaging/MessagingGateway.html) to [EventStore](https://geteventstore.com/) streams. ###

By adding this plugin to an application, such as a Windows service, the application is able to connect to EventStore; subscribe to and handle [events](http://www.enterpriseintegrationpatterns.com/patterns/messaging/EventMessage.html) from named [streams](http://www.enterpriseintegrationpatterns.com/patterns/messaging/MessageChannel.html); persist an aggregate to, and rehydrate it from, a stream, as well as populating a read model.

## Requirements ##

An instance of the EventStore server should be running on the network. Please follow the [installation](http://docs.geteventstore.com/introduction/) instructions provided by EventStore.

You can verify that EventStore is running by browsing to port **2113** on the machine running the EventStore server.

## Getting Started ##

Add a reference to **ServiceStack.EventStore** in your project. 

### Setting up a Connection to EventStore ###
Add the following code to the `Configure` method in your `AppHost` class (this class is created automatically for you when you use one of the ServiceStack project templates):

```csharp
public override void Configure(Container container)
{
	var connection = new EventStoreConnectionSettings()
   							.UserName("admin")
            				.Password("changeit")
            				.TcpEndpoint("localhost:1113");
	
	Plugins.Add(new EventStoreFeature(connection));
}
```
Additionally, you can take advantage of the ServiceStack `MetadataFeature` to provide a link to the EventStore admin UI by providing the HTTP address of the EventStore instance:

```csharp
public override void Configure(Container container)
{
    var connection = new EventStoreConnectionSettings()
                            .UserName("admin")
                            .Password("changeit")
                            .TcpEndpoint("localhost:1113")
							.HttpEndpoint(localhost:2113");
    
    Plugins.Add(new EventStoreFeature(connection));
    Plugins.Add(new MetadataFeature());
}
```
**Please note** that this sample assumes that:

- EventStore is running on your **local host**. **1113** is the TCP port at which you can listen for events and **2113** is the HTTP port.

### Subscribing to Named Streams ###

There are four different kinds of subscription to streams that ServiceStack.EventStore can create:

<table class="tg">
  <tr>
    <th class="tg-qnmb">Subscription Type</th>
    <th class="tg-qnmb">Description</th>
    <th class="tg-qv16">Expected Parameters</th>
  </tr>
  <tr>
    <td class="tg-9hbo">Volatile</td>
    <td class="tg-n9nb">Provides access to an EventStore volatile subscription which starts reading from the next event following connection on a named stream.</td>
    <td class="tg-yw4l">The stream name.</td>
  </tr>
  <tr>
    <td class="tg-e3zv">Persistent</td>
    <td class="tg-381c">Provides access to an EventStore persistent subscription which supports the <a href="http://www.enterpriseintegrationpatterns.com/patterns/messaging/CompetingConsumers.html">competing consumer</a> messaging model on a named stream.</td>
    <td class="tg-yw4l">The stream name and the subscription group.</td>
  </tr>
  <tr>
    <td class="tg-e3zv">Catch-Up</td>
    <td class="tg-031e">Provides access to an EventStore catch-up subscription which starts reading from either the beginning of a named stream or from a specified event number on that stream.</td>
    <td class="tg-yw4l">The stream name.</td>
  </tr>
  <tr>
    <td class="tg-e3zv">Read Model</td>
    <td class="tg-031e">Also provides access to an EventStore catch-up subscription with the difference that it automatically subscribes to all ("$all" in EventStore) to allow a read model to be populated from selected events from different streams.</td>
    <td class="tg-yw4l">None.</td>
  </tr>
</table>

A subscription can be created in the following way in the `Configure` method:

```csharp
public override void Configure(Container container)
{
    var settings = new SubscriptionSettings()
			        	.SubscribeToStreams(streams =>
            	    	{
                	    	streams.Add(new CatchUpSubscription("deadletterchannel"));
                		});

 	...Connection set-up omitted

	// Note the extra parameter being used when creating an instance of the EventStoreFeature
	Plugins.Add(new EventStoreFeature(connection, settings));
}
```

Multiple subscriptions can be added here.

#### Handling Events ####

This plugin makes use of ServiceStack's architecture to route events from EventStore streams to their handlers. 

To handle an event on a stream that you have subscribed to simply create a class that handles from `ServiceStack.Service` and add an end point for the event you wish to handle:

```csharp
public class PurchaseOrderService : Service
{
    public object Any(PurchaseOrderCreated @event)
    {
		...handle event
    }

    public object Any(OrderLineItemsAdded @event)
    {
		...handle event
    }
}
```
#### Setting a Retry Policy ####

When creating a subscription you can also specify the retry policy used by ServiceStack.EventStore in response to a subscription to EventStore beimg dropped. Since the retry functionality builds on the <a href="https://github.com/App-vNext/Polly">Polly</a> library the retry policy can be set by either specify an `IEnumerable<TimeSpan>` or a delegate.

To create a volatile subscription to a named stream that, if the subscription is dropped, attempts to resubscribe according to specified durations add the following code to your `Configure` method:

```csharp
var settings = new SubscriptionSettings()
		            .SubscribeToStreams(streams =>
        	        {
            	        // using an IEnumerable<TimeSpan>
                    	streams.Add(new CatchUpSubscription("deadletterchannel")
                        	.SetRetryPolicy(new[] {1.Seconds(), 3.Seconds(), 5.Seconds()}));
                	});
```
If we wanted to specify an algorithm to allow for an exponential back-off we can pass in a delegate:

```csharp
var settings = new SubscriptionSettings()
	                .SubscribeToStreams(streams =>
    	            {
						// using a delegate
                    	streams.Add(new CatchUpSubscription("deadletterchannel")
                        			.SetRetryPolicy(
                        				10.Retries(), 
                                		retryCounter => TimeSpan.FromSeconds(Math.Pow(2, retryCounter)))
                            		);
                    });
```
#### Configuring a Read Model Subscription ####

As mentioned before, a read model subscription is built on top of a catch-up subscription. In addition to a catch-up subscription a read model storage must be specified. 

So far, the only storage model that has been made available is <a href="http://redis.io/">Redis</a>:

```csharp
var settings = new SubscriptionSettings()
                	.SubscribeToStreams(streams =>
                	{
                    	streams.Add(new ReadModelSubscription()
                                    .SetRetryPolicy(new [] {1.Seconds(), 3.Seconds()})
                                    .WithStorage(new ReadModelStorage(StorageType.Redis, "localhost:6379")));
	                });
```
**Please note** that this code assumes that you have an instance of Redis installed on your local host and which is using port 6379.

#### Populating a Read Model ####

To populate a read model from EventStore you should **(1)** add a `ReadModelSubscription` as demonstrated in the previous code snippet, **(2)** create a view model class to represent a record in the read model, and then **(3)** instantiate a `ProjectionWriter`, specifying the type of the unique Id and the view model to be used, in the service you are using to handle events.

When handling an event that corresponds to a new record being required in the read model - for example, `PurchaseOrderCreated` - then use the `Add` method to create a new instance of desired view model. When handling events that should update the state of a record in the read model then use the `Update` method to pass in the Id of the record to be updated as well as a delegate that mutates the appropriate properties of the view model:

```csharp
public class PurchaseOrderService : Service
{
    private IProjectionWriter<Guid, OrderViewModel> writer = 
                        ProjectionWriterFactory.GetRedisClient<Guid, OrderViewModel>();

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
```

## Attributions ##

This project leans partly on the following OS projects:

- <a href="https://github.com/mfelicio/NDomain">**NDomain**</a> by Manuel Felício
- <a href="https://github.com/gnschenker/EventSourcing">**EventSourcing**</a> by Gabriel Shenker
- <a href="https://github.com/EventStore/getting-started-with-event-store">**Getting-Started-With-Event-Store**</a> byJames Nugent 




