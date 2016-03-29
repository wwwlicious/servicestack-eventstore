using System;

namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    public class WeeGreenManService: Service
    {
        public void Any(WeeGreenMenLanded @event)
        {
            Console.WriteLine("here");
        }
    }
}
