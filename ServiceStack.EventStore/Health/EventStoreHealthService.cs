
namespace ServiceStack.EventStore.Health
{
    using System;

    //todo: will expose the state of health of the plugin
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
