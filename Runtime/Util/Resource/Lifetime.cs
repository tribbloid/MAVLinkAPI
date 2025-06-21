using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32.SafeHandles;
using UnityEngine;

namespace MAVLinkAPI.Util.Resource
{
    public class Registry : SafeHandleMinusOneIsInvalid
    {
        public Registry(bool ownsHandle) : base(ownsHandle)
        {
        }

        protected override bool ReleaseHandle()
        {
            return true;
        }

        // Using HashSet with a lock for thread safety
        // also the AccessLock
        public readonly ConcurrentDictionary<int, Cleanable> Managed = new();


        // Methods to add and remove Cleanable objects with thread safety
        public virtual void Register(Cleanable cleanable)
        {
            lock (Managed)
            {
                Managed.GetOrAdd(cleanable.ID, cleanable);
            }
        }

        public virtual void Deregister(Cleanable cleanable)
        {
            lock (Managed)
            {
                Managed.Remove(cleanable.ID, out _);
            }
        }

        public List<T> CollectByType<T>() where T : class
        {
            lock (Managed)
            {
                return Managed.Values.OfType<T>().ToList();
            }
        }

        public List<Cleanable> CollectByType(Type type)
        {
            lock (Managed)
            {
                return Managed.Values.Where(x => type.IsAssignableFrom(x.GetType())).ToList();
            }
        }

        public static Registry Global => new Registry(false);

        public static object GlobalAccessLock => Global.Managed;
    }

    public class Lifetime : Registry
    {
        // TODO: need lifetime semilattice algebra:
        // - earlier of two (<'a, 'b>)
        // - later of two (<'a, 'b, 'c> where 'a: 'c, 'b: 'c)

        private static readonly IntPtr ValidHandle = new(0);
        // private static readonly IntPtr InvalidHandle = new(-1);


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