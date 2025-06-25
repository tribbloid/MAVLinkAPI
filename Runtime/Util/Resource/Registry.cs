using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32.SafeHandles;

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

        public static Registry Global => new(false);

        public static object GlobalAccessLock => Global.Managed;
    }
}