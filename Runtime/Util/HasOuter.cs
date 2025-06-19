namespace MAVLinkAPI.Util
{
    public abstract class HasOuter<T>
    {
        public readonly T Outer;

        protected HasOuter(T outer)
        {
            Outer = outer;
        }
    }
}