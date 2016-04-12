# ServiceStack.EventStore #

[![Build status](https://ci.appveyor.com/api/projects/status/v9qd6kso0bkc5spf/branch/master?svg=true)](https://ci.appveyor.com/project/wwwlicious/servicestack-eventstore/branch/master)
[![NuGet version](https://badge.fury.io/nu/ServiceStack.EventStore.svg)](https://badge.fury.io/nu/ServiceStack.EventStore)

A plugin for [ServiceStack](https://servicestack.net/) that provides a [message gateway](http://www.enterpriseintegrationpatterns.com/patterns/messaging/MessagingGateway.html) to [EventStore](https://geteventstore.com/) streams.

By adding this plugin to an application, such as a Windows Service, the application is able to connect to EventStore; subscribe to and handle [events](http://www.enterpriseintegrationpatterns.com/patterns/messaging/EventMessage.html) from named [streams](http://www.enterpriseintegrationpatterns.com/patterns/messaging/MessageChannel.html); persist an aggregate to, and rehydrate it from, a stream; as well as populating a read model.

## Requirements ##

An instance of the EventStore server should be running on the network. Please follow the [installation](http://docs.geteventstore.com/introduction/) instructions provided by EventStore.

You can verify that EventStore is running by browsing to port <a href="http://localhost:2113/">**2113**</a> on the machine running the EventStore server.

## Getting Started ##

Install the package from [Nuget](https://www.nuget.org/packages/ServiceStack.EventStore/)
```bash
Install-Package ServiceStack.EventStore
```

### Setting up a Connection to EventStore ###
Add the following code to the `Configure` method in the `AppHost` class (this class is created automatically when you use one of the ServiceStack project templates). Additionally, you can take advantage of the ServiceStack `MetadataFeature` to provide a link to the EventStore admin UI by providing the HTTP address of the EventStore instance:

```csharp
public override void Configure(Container container)
{
	var connection = new EventStoreConnectionSettings()
					.UserName("admin")
					.Password("changeit")
					.TcpEndpoint("localhost:1113")
                    .HttpEndpoint("localhost:2113");
                    
	//Register the EventStore plugin with ServiceStack, passing in the connection 
	//and the assembly that contains the CLR events (see below)
	Plugins.Add(new EventStoreFeature(connection, typeof(ClrEvent).Assembly)); 
	//Optionally register the Metadata plugin
    Plugins.Add(new MetadataFeature());
}
```

**Please note** that this sample assumes that:

- EventStore is running on your **local host**. **1113** is the TCP port at which you can listen for events and **2113** is the HTTP port. These are the default ports that EventStore uses.

### Subscribing to Named Streams ###

There are four different kinds of subscriptions to streams that ServiceStack.EventStore can create:

<table class="tg">
  <tr>
    <th class="tg-qnmb">Subscription Type</th>
    <th class="tg-qnmb">Description</th>
    <th class="tg-qv16">Expected Parameters</th>
  </tr>
  <tr>
    <td class="tg-9hbo">Volatile</td>
    <td class="tg-n9nb">Provides access to an EventStore volatile subscription, which starts reading from the next event published on a named stream following successful connection by the plugin.</td>
    <td class="tg-yw4l">The stream name.</td>
  </tr>
  <tr>
    <td class="tg-e3zv">Persistent</td>
    <td class="tg-381c">Provides access to an EventStore persistent subscription, which supports the competing consumers messaging model on a named stream.</td>
    <td class="tg-yw4l">The stream name and the subscription group.</td>
  </tr>
  <tr>
    <td class="tg-e3zv">Catch-Up</td>
    <td class="tg-031e">Provides access to an EventStore catch-up subscription, which starts reading from either the beginning of a named stream or from a specified event number on that stream.</td>
    <td class="tg-yw4l">The stream name.</td>
  </tr>
  <tr>
    <td class="tg-e3zv">Read Model</td>
    <td class="tg-031e">Also provides access to an EventStore catch-up subscription, with the difference that it automatically subscribes to **all** streams ("$all" in EventStore) to enable a read model to be populated from selected events from different streams.</td>
    <td class="tg-yw4l">None.</td>
  </tr>
</table>

Subscriptions can be created as follows in the `Configure` method (we will cover read model subscriptions separately):

```csharp
public override void Configure(Container container)
{
    var settings = new SubscriptionSettings()
			        	.SubscribeToStreams(streams =>
            	    	{
            	    		//read a stream from the first event
                	    	streams.Add(new CatchUpSubscription("stream_name"));
                            //read a stream from this moment forward
                            streams.Add(new VolatileSubscription("stream_name"));
                            //receive events from a stream as a competing consumer
                            streams.Add(new PersistentSubscription("stream_name", "subscription_group_name"));
                		});

 	...connection set-up omitted

	// Note the extra 'settings' parameter being used when creating an instance of the EventStoreFeature
	Plugins.Add(new EventStoreFeature(connection, settings, typeof(ClrEvent).Assembly));
}
```

#### Modelling Events ####

The content of events in EventStore is stored in JSON format. In a language based on the .Net CLR we model each type of event that we want to work with as a class:
```csharp
public class OrderCreated
{
	public Guid OrderId {get; set;}
   	public DateTime Created {get; set;}
}
```
There is no need for such a class to implement a particular interface or inherit from a parent class. Rather, as you have seen, when registering the EventStore plugin we pass in a reference to the assembly (or assemblies) that contain the relevant classes:

```csharp
public override void Configure(Container container)
{
	...
	Plugins.Add(new EventStoreFeature(connection, settings, typeof(SomeEvent).Assembly, typeof(AnotherEvent).Assembly);
}
```

#### Publishing Events ####

By adding the ServiceStack.EventStore package to your project we can access the `EventStoreRepository` through constructor injection and use it to publish asynchronously to a named stream:

```csharp

public class FlightService: ServiceStack.Service
{
	private readonly IEventStoreRepository repository;
	
	public void FlightService(IEventStoreRepository repo)
	{
	    this.repository = repo;    
	}   
	
	public async Task DoSomething()
	{
	    ...
	    await repository.PublishAsync(new SomethingHappened(), "targetstream");
	}
}

```

Additionally, we can set the headers for the event:

```csharp

public async Task DoSomething()
{
    ...
    await _repo.PublishAsync(new SomethingHappened(), "targetstream",
		        headers =>
		        {
		            headers.Add("CorrelationId", correlationId);
		            headers.Add("SitarPlayer", "Ustad Vilyat Khan");
		        });
}
```
#### Handling Events ####

This plugin makes use of ServiceStack's architecture to route events from EventStore streams to their handlers which are implemented as methods on a service class: 

![Routing Events](https://github.com/MacLeanElectrical/servicestack-eventstore/blob/master/assets/RoutingEvents.png)

To handle an event on a stream to which you have subscribed simply create a class that inherits from `ServiceStack.Service` and add an endpoint for each event you wish to handle:

```csharp
public class PurchaseOrderService : Service
{
    public object Any(PurchaseOrderCreated @event)
    {
		//handle event
    }

    public object Any(OrderLineItemsAdded @event)
    {
		//handle event
    }
}
```

Headers for an event can be retrieved through the `Headers` property of the `Request` object that ServiceStack exposes in any class that inherits from `Service`:

```csharp
public class PurchaseOrderService: Service
{
	public object Any(PurchaseOrderCreated @event)
	{
		var correlationId = Request.Headers["CorrelationId"];
		//handle event
	}
}
```
#### Setting a Retry Policy ####

When creating a subscription you can also specify the retry policy used by ServiceStack.EventStore in response to a subscription to EventStore being dropped. Since the retry functionality builds on the <a href="https://github.com/App-vNext/Polly">Polly</a> library, the retry policy can be set by either specifying a parameter array `TimeSpan` or a delegate.

For example, in the `Configure` method we can specify a series of `TimeSpan`s that tell the plugin that in the event of a specified subscription being dropped it should wait one second before retrying the subscription. And then three seconds after that. And then five seconds after that:

```csharp
var settings = new SubscriptionSettings()
		            .SubscribeToStreams(streams =>
        	        {
                    	streams.Add(new VolatileSubscription("deadletterchannel")
                        	.SetRetryPolicy(1.Seconds(), 3.Seconds(), 5.Seconds()));
                	});
```
Alternatively, we can also tell the plugin to use an <a href="https://en.wikipedia.org/wiki/Exponential_backoff">exponential back-off</a> to multiplicatively increase the time to wait, for a specified maximum number of retry attempts, before attempting to resubscribe:

```csharp
var settings = new SubscriptionSettings()
	                .SubscribeToStreams(streams =>
    	            {
                    	streams.Add(new VolatileSubscription("deadletterchannel")
                        			.SetRetryPolicy(
                        				10.Retries(), 
                                		retryCounter => TimeSpan.FromSeconds(Math.Pow(2, retryCounter)))
                            		);
                    });
```

#### Aggregates ####

This plugin supports the event-sourced [aggregates](http://martinfowler.com/bliki/DDD_Aggregate.html) pattern whereby the state of an aggregate object is mutated by means of events that are raised in response to commands executed against that aggregate. Every event is raised in response to a command is held in memory until the aggregate is persisted to the event store. Following the event-sourcing mode, it is not the state of the aggregate that is persisted but, rather, the events which have led the aggregate to be in its current state. 

When the aggregate is loaded (or 'rehydrated' in the parlance of event sourcing), again, it is not the state as such of the aggregate that is loaded but, rather, the events which were previously persisted to the event store. These events are re-applied to the aggregate (in exactly the same way they were when the original commands were executed) to reach the proper state of the aggregate. As Greg Young has reiterated "Current State is a [left fold](https://en.wikipedia.org/wiki/Fold_%28higher-order_function%29) of previous facts". 

![Event Sourced Aggregate](https://github.com/MacLeanElectrical/servicestack-eventstore/blob/master/assets/Aggregate.png)

In many implementations of the event-sourced aggregate pattern to be found on the internet (such as [here](https://lostechies.com/gabrielschenker/2015/06/06/event-sourcing-applied-the-aggregate/), [here](http://danielwhittaker.me/2014/11/15/aggregate-root-cqrs-event-sourcing/), and [here](http://bit.ly/1YhgPCR)) the aggregate is modelled as a **single** class exposing (1) API methods that raise events in response to commands, (2) event handlers that mutate state in response to these events being raised, and (3) fields that hold that state. ServiceStack.EventStore, however, supports the modelling of a (logical) aggregate as two distinct classes with single responsibilities: a class that inherits from `Aggregate<TState>` and exposes command methods that are responsible for validation of the commands and raising events in response to them.

An event is raised by using the `Causes<TEvent>` method:

```csharp
public class Flight : Aggregate<FlightState>
{
	public Flight(Guid id) : base(id)
	{
	}
	
	public Flight(): base(Guid.NewGuid())
	{
	    Causes(new FlightCreated(Id));
	}
	
	public void UpdateFlightNumber(string newFlightNumber)
	{
		if (!string.IsNullOrEmpty(destination))
			Causes(new FlightNumberUpdated(newFlightNumber));
	}
	
	public void UpdateDestination(string destination)
	{
		if (!string.IsNullOrEmpty(destination))
			Causes(new DestinationChanged(destination));
	}
}

```

And a class that inherits from `State` that encapsulates the state of the aggregate and implements handlers for the events raised and mutates the state. An event `SomethingHappened` raised in the class that inherits `Aggregate<TState>` is handled by simply implementing a method `On(SomethingHappened @event)`. The state of an aggregate should almost always be mutated by means of raising events and for that reason it is recommended that the fields of a state object be set as `{get; private set;}`:

```csharp
public class FlightState : State
{
	public string FlightNumber { get; private set; }
	public string Destination { get; private set; }
	
	public void On(FlightCreated @event)
	{
	}
	
	public void On(FlightNumberUpdated @event)
	{
	    FlightNumber = @event.NewFlightNumber;
	}
	
	public void On(DestinationChanged @event)
	{
	    Destination = @event.Destination;
	}
}
```


```csharp

public class FlightService
{
	private readonly IEventStoreRepository repo;
	
	public void FlightService(IEventStoreRepository repo)
	{
		this.repo = repo;
	}
	
	public async Task CancelFlight(Guid flightId)
	{
		var flight = await repo.GetByIdAsync<Flight>(flightId)
				       .ConfigureAwait(false);
		
		flight.UpdateDestination("Dingwall International Airport");
		
		await repo.SaveAsync(flight);
	}
}
```

#### Configuring a Read Model Subscription ####

As mentioned previously, a read model subscription is similar to a catch-up subscription, with the difference being that a read model subscription subscribes to **all** streams in EventStore (to the `$all` projection) and, further, requires that a storage mechanism for the read model be specified. 

A read model is essentially a projection of all events, or a subset thereof, that provides a stateful view of these events in a way that adds value to the end-users of the system.     

Currently, the only storage model that is available is [Redis](http://redis.io/):

```csharp
var settings = new SubscriptionSettings()
                	.SubscribeToStreams(streams =>
                	{
                    	streams.Add(new ReadModelSubscription()
                                    .SetRetryPolicy(1.Seconds(), 3.Seconds())
                                    .WithStorage(new ReadModelStorage(StorageType.Redis, "localhost:6379")));
	                });
```
**Please note** that this code sample assumes that you have an instance of Redis installed on your local host which is using port [**6379**](http://localhost:6379/). Windows users can download the latest version of Redis from [MSOpenTech](https://github.com/MSOpenTech/redis/releases) or install it from [Chocolatey](https://chocolatey.org/packages/redis-64/).

#### Populating a Read Model ####

To populate a read model from subscribed event streams you need to do the following:

* Create a `ReadModelSubscription` in the `Configure` method of the `AppHost`, as demonstrated above.
* For each event type that you wish to consume from EventStore create a class. There is no need for such a class to implement an interface.
* Create a class that inherits from `ServiceStack.Service` and add methods that take in the desired CLR event types. 
* Create a view model class to represent a record in the read model. Potentially, this could be a hierarchical object graph that could be persisted as a JSON document in Redis (or RavenDB ) or as a set of rows in RDBMS tables.  
* In the `Service` class instantiate a `ProjectionWriter`, specifying the type of the unique Id and the view model to be used.

#### Creating a Projection ####

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

This project leans gratefully on the following OS projects:

- [**NDomain**](https://github.com/mfelicio/NDomain) by Manuel Felício
- [**EventSourcing**](https://github.com/gnschenker/EventSourcing) by Gabriel Shenker
- [**Getting-Started-With-Event-Store**](https://github.com/EventStore/getting-started-with-event-store) by James Nugent 
