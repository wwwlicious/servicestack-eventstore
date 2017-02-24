// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    using global::EventStore.ClientAPI.Embedded;

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
