namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    public class ServiceStackHostFixture
    {
        private const string ListeningOn = "http://localhost:8088/";

        public ServiceStackHostFixture()
        {
            var appHost = new TestAppHost();
            appHost.Init();

            if (!appHost.HasStarted)
            {
                appHost.Start(ListeningOn);
            }

            AppHost = appHost;
        }

        public TestAppHost AppHost { get; }

        ~ServiceStackHostFixture()
        {
            AppHost.Dispose();
        }
    }
}
