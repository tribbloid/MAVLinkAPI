#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MAVLinkAPI.log4net;
using UnityEngine;
using Random = System.Random;

namespace MAVLinkAPI.Util.Resource
{
    public abstract class Cleanable : IDisposable
    {
        // public static class GlobalRegistry
        // {
        //     public static readonly AtomicInt GlobalCounter = new();
        //
        //     // TODO: not efficient, should use ConcurrentMultiMap
        //     public static readonly HashSet<Cleanable> Registered = new();
        // }


        public int ID = new Random().Next();
        public DateTime CreatedAt = DateTime.UtcNow;

        private readonly Lifetime _lifetime;

        public Cleanable(Lifetime? lifetime = null)
        {
            lifetime ??= Lifetime.Static;

            _lifetime = lifetime;
            _lifetime.Register(this);

            Registry.Global.Register(this);
        }

        public bool IsDisposed = false;

        public void Dispose()
        {
            try
            {
                DoClean();
                IsDisposed = true;
                _lifetime.Deregister(this);
                LogManager.GetLogger(GetType()).Info("disposing " + GetType().Name);
                Debug.Log("disposing " + GetType().Name);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(GetType()).Error(e);
            }
        }

        ~Cleanable()
        {
            Dispose();
        }

        public abstract void DoClean();

        public virtual string GetStatusSummary()
        {
            var vType = GetType();
            var text = vType.Name;
            return text;
        }

        public virtual List<string> GetStatusDetail()
        {
            var uptime = DateTime.UtcNow - CreatedAt;

            return new List<string>()
            {
                ToString(),
                $"- ID: {ID}",
                $"- Uptime: {uptime}"
            };
        }


        public class Dummy : Cleanable
        {
            public Dummy(
                Lifetime? lifetime = null
            ) : base(lifetime)
            {
            }

            public override void DoClean()
            {
            }
        }
    }


    public static class SafeCleanExtensions
    {
        // public static object AccessLock<T>(this T self) where T : Cleanable
        // {
        //     return Cleanable.GlobalAccessLock; // this is very blocking, will optimise later
        // }

        public static IEnumerable<T> SelfAndPeers<T>(this T self)
            where T : Cleanable // should be read only
        {
            lock (Registry.Global.Managed)
            {
                var selfType = self.GetType();

                var filtered = Registry.Global.CollectByType(selfType);

                return filtered.Cast<T>();
            }
        }


        public static IEnumerable<T> Peers<T>(this T self) where T : Cleanable
        {
            var peers = self.SelfAndPeers<T>();
            return peers.Where(v => !ReferenceEquals(v, self));
        }
    }
}