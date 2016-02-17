namespace ServiceStack.EventStore.GuaranteedDelivery
{
    using System.Collections.Generic;

    public class InMemoryStorage: IStoreAndForwardMechanism
    {
        private Queue<IEvent> eventQueue;

    }
}
