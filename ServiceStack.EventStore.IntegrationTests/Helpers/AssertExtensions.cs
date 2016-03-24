namespace ServiceStack.EventStore.IntegrationTests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Xunit;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public static class AssertExtensions
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        public static bool HasAttribute<TAttribute>(this MethodInfo method) where TAttribute : Attribute
        {
            return method.GetCustomAttributes(typeof(TAttribute), false).Any();
        }

        public static IEnumerable<MethodInfo> GetAsyncVoidMethods(this Assembly assembly)
        {
            return assembly.GetLoadableTypes()
              .SelectMany(type => type.GetMethods(
                BindingFlags.NonPublic
                | BindingFlags.Public
                | BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.DeclaredOnly))
              .Where(method => method.HasAttribute<AsyncStateMachineAttribute>())
              .Where(method => method.ReturnType == typeof(void));
        }
        public static void AssertNoAsyncVoidMethods(Assembly assembly)
        {
            var messages = assembly
                .GetAsyncVoidMethods()
                .Select(method =>
                    $"'{method.DeclaringType?.Name}.{method.Name}' is an async void method.")
                .ToList();

            Assert.False(messages.Any(),
                "Async void methods found!" + Environment.NewLine + string.Join(Environment.NewLine, messages));
        }
    }
}
