using System;

namespace MAVLinkAPI.Util
{
    /**
     * pure function
     * definition with more argument(s) will come later
     */
    public static class Pure
    {
        public class Unary<T>
        {
            public Func<T> Self;
        }

        public static class Unary
        {
            public class ToStruct<T> : Unary<T> where T : struct
            {
            }

            public class ToClass<T> : Unary<T> where T : class
            {
            }
        }

        // TODO: polymorphism doesn't work at the moment
        // public static Unary.ToStruct<T> Of<T>(Func<T> self) where T : struct
        // {
        //     return new Unary.ToStruct<T> { Self = self };
        // }
        //
        // public static Unary.ToClass<T> Of<T>(Func<T> self) where T : class
        // {
        //     return new Unary.ToClass<T> { Self = self };
        // }
    }
}