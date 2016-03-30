namespace ServiceStack.EventStore.Extensions
{
    using System.Threading.Tasks;

    /// <summary>
    /// Extensions for working with Task
    /// </summary>
    internal static class TaskExtensions
    {
        //Represents a succesfully completed Task
        public static readonly Task CompletedTask = Task.FromResult(false);
    }
}
