using System;
using UnityEngine;

namespace MAVLinkAPI.Util.Resource
{
    public class Lifetime : Registry
    {
        // TODO: need lifetime semilattice algebra:
        // - earlier of two (<'a, 'b>)
        // - later of two (<'a, 'b, 'c> where 'a: 'c, 'b: 'c)

        private static readonly IntPtr ValidHandle = new(0);

        public Lifetime(
            IntPtr? handle = null,
            bool ownsHandle = true
        ) : base(ownsHandle)
        {
            handle ??= ValidHandle;

            SetHandle(handle.Value);
        }

        protected sealed override bool ReleaseHandle()
        {
            return DeregisterAll();
        }


        public bool DeregisterAll()
        {
            var noError = true;

            lock (Managed)
            {
                foreach (var cleanable in Managed.Values)
                    try
                    {
                        cleanable.Dispose();
                        Deregister(cleanable);
                        Debug.Log($"successfully cleaned {cleanable}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("cleaning failed: " + e);
                        noError = false;
                    }
            }

            return noError;
        }

        // Operator overloads for += and -= syntax
        public static Lifetime operator +(Lifetime lifetime, Cleanable cleanable)
        {
            lifetime.Register(cleanable);
            return lifetime;
        }

        public static Lifetime operator -(Lifetime lifetime, Cleanable cleanable)
        {
            lifetime.Deregister(cleanable);
            return lifetime;
        }

        // Static lifetime that can be used application-wide
        public static readonly Lifetime Static = new TStatic();

        // Internal implementation for the static lifetime
        private class TStatic : Lifetime
        {
            public TStatic() : base()
            {
            }
        }
    }
}