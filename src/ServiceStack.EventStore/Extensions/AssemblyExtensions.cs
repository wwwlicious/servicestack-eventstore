using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ServiceStack.EventStore.Extensions
{
    internal static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetVisibleClasses(this IEnumerable<Assembly> @this) => 
                        @this.SelectMany(a => Assembly.Load(a.GetName()).GetTypes()
                              .Where(t => t.IsClass && t.IsVisible));
     
    }
}
