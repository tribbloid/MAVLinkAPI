namespace MAVLinkAPI.Util
{
    public abstract class HasOuter<T>
    {
        public T Outer { get; init; }

        // protected HasOuter(T outer)
        // {
        //     Outer = outer;
        // }
    }
}