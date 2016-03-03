namespace ServiceStack.EventStore
{
    using System;

    [Route("/healthcheck")]
    public class EventStoreHealthService: Service
    {
        public void Any(EventStoreCheck check)
        {
            //logic
            throw new NotImplementedException();
        }
    }
}
