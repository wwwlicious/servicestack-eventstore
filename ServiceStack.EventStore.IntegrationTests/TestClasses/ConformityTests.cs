namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    using Xunit;
    using System.Reflection;
    using AssertExtensions = Helpers.AssertExtensions;

    public class ConformityTests
    {

        //todo this is wrongly picking up methods that return async Task - needs tweaking.
        public void EnsureNoAsyncVoidTests()
        {
            var assemblyUnderTest = Assembly.Load("ServiceStack.EventStore");
            AssertExtensions.AssertNoAsyncVoidMethods(assemblyUnderTest);
        }
    }
}
