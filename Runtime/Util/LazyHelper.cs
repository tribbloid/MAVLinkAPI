#nullable enable
using System;
using System.Threading;

namespace MAVLinkAPI.Util
{
    public static class LazyHelper
    {
        // TODO: too verbose, should move to Maybe<T>.Lazy after testing

        public static T EnsureInitialized<T>(ref T? sym, Func<T> fn) where T : class
        {
            var result = LazyInitializer.EnsureInitialized(ref sym, fn);
            return result!;
        }

        public static T EnsureInitialized<T>(ref Box<T>? sym, Func<T> fn) where T : struct
        {
            var boxed = LazyInitializer.EnsureInitialized(ref sym, () => new Box<T>(fn()));
            var result = boxed!.Value;

            return result;
        }
    }

    // public static class LazyExtensions
    // {
    //     public static T BindAsLazy<T>(this Func<T> fn, ref T? sym) where T : class
    //     {
    //         return LazyHelper.EnsureInitialized(ref sym!, fn);
    //     }
    //
    //     public static T BindAsLazy<T>(this Func<T> fn, ref Box<T>? sym) where T : struct
    //     {
    //         return LazyHelper.EnsureInitialized(ref sym!, fn);
    //     }
    // }
}