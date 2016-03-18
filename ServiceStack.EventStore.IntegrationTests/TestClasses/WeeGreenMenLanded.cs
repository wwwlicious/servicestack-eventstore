using System;
using ServiceStack.EventStore.Types;

namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    public class WeeGreenMenLanded: Event
    {
        public Guid Id { get; }
        public int NoOfWeeGreenMen { get; }
        public DateTime TimeStamp { get; }

        public WeeGreenMenLanded(int noOfWeeGreenMen): base("alien-landings")
        {
            Id = Guid.NewGuid();
            NoOfWeeGreenMen = noOfWeeGreenMen;
            TimeStamp = DateTime.UtcNow;
        }
    }
}