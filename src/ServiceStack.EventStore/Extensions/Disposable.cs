namespace ServiceStack.EventStore.Extensions
{
    using System;

    //Dave Fancher's Disposable extensions to allow method chaining
    public static class Disposable
    {
        public static TResult Using<TDisposable, TResult>
        (
          Func<TDisposable> factory, Func<TDisposable, TResult> fn) where TDisposable : IDisposable
            {
                using (var disposable = factory())
                {
                    return fn(disposable);
                }
            }
    }
}
