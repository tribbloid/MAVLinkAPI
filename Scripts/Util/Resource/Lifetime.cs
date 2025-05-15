using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using UnityEngine;

namespace MAVLinkAPI.Scripts.Util.Resource
{
    public class Lifetime : SafeHandleMinusOneIsInvalid
    {
        // TODO: need lifetime semilattice algebra:
        // - earlier of two (<'a, 'b>)
        // - later of two (<'a, 'b, 'c> where 'a: 'c, 'b: 'c)

        private static readonly IntPtr ValidHandle = new(0);
        // private static readonly IntPtr InvalidHandle = new(-1);

        // Using HashSet with a lock for thread safety
        public readonly ConcurrentDictionary<int, Cleanable> Managed = new();
        // private readonly object _accessLock = new();

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

        // Methods to add and remove Cleanable objects with thread safety
        public void Register(Cleanable cleanable)
        {
            lock (Managed)
            {
                Managed.GetOrAdd(cleanable.ID, cleanable);
            }
        }

        public void Deregister(Cleanable cleanable)
        {
            lock (Managed)
            {
                Managed.Remove(cleanable.ID, out _);
            }
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
        public static readonly Lifetime Static = new StaticLifetime();

        // Internal implementation for the static lifetime
        private class StaticLifetime : Lifetime
        {
            public StaticLifetime() : base()
            {
            }
        }
    }


    // public class Fallback : Lifetime
    // {
    // }
}