// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.EventStore.HelperClasses
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Instantiates TClass using a compiled lambda expression and caches it. This assumes a parameterless constructor.
    /// </summary>
    /// <typeparam name="TClass">The type of the class to be instantiated.</typeparam>
    internal static class New<TClass> where TClass : class
    {
        public static readonly Func<TClass> Instance = Expression.Lambda<Func<TClass>>(Expression.New(typeof(TClass))).Compile();

        public static Func<TClass> GetInstance(TClass type) => Instance;

        /// <summary>
        /// Allows a class to be instantiated using a parameterised constructor
        /// </summary>
        /// <typeparam name="TParam"></typeparam>
        internal static class WithCtorParam<TParam>
        {
            private static readonly Dictionary<Type, Func<TParam, TClass>> cache = new Dictionary<Type, Func<TParam, TClass>>();

            public static TClass Instance(TParam arg) 
            {
                Func<TParam, TClass> cachedFunc;

                if (cache.TryGetValue(typeof(TClass), out cachedFunc))
                {
                    return cachedFunc(arg);
                }

                var constructor = typeof(TClass).GetConstructor(new[] { typeof(TParam) });
                var paramExpression = Expression.Parameter(typeof(TParam));

                var func = Expression.Lambda<Func<TParam, TClass>>(Expression.New(constructor, paramExpression), paramExpression).Compile();
                cache.Add(typeof(TClass), func);

                return func(arg);
            }
        }
    }

    /// <summary>
    /// Instantiates a class using generic parameters passed in at runtime.
    /// </summary>
    internal static class New
    {
        private static readonly Dictionary<Tuple<Type, Type>, Func<object>> cache = new Dictionary<Tuple<Type, Type>, Func<object>>();

        public static object CreateGenericInstance(Type genericTypeDefinition, Type genericParameter)
        {
            Func<object> instanceCreator;

            var cacheKey = Tuple.Create(genericTypeDefinition, genericParameter);

            if (!cache.TryGetValue(cacheKey, out instanceCreator))
            {
                var genericType = genericTypeDefinition.MakeGenericType(genericParameter);
                instanceCreator = Expression.Lambda<Func<object>>(Expression.New(genericType)).Compile();
                cache[cacheKey] = instanceCreator;
            }

            return instanceCreator();
        }
    }
}
