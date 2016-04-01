namespace ServiceStack.EventStore.IntegrationTests.TestDomain
{
    public class PassengerAdded
    {
        public Passenger Passenger { get; set; }
        public int NoOfBags { get; set; }
    }
}