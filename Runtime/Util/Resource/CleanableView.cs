#nullable enable
using System;

namespace MAVLinkAPI.Util.Resource
{
    public class CleanableView<T> : Cleanable where T : IDisposable
    {
        public T Value;

        public static implicit operator T(CleanableView<T>? view)
        {
            return view != null ? view.Value : default!;
        }

        public CleanableView(T value, Lifetime? lifetime = null) : base(lifetime)
        {
            Value = value;
        }

        public override void DoClean()
        {
            Value.Dispose();
        }
    }
}