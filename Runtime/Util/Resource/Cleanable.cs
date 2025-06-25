#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace MAVLinkAPI.Util.Resource
{
    public abstract class Cleanable : IDisposable
    {
        public int ID = new Random().Next();
        public DateTime CreatedAt = DateTime.UtcNow;

        private readonly Lifetime _lifetime;

        protected Cleanable(Lifetime? lifetime = null)
        {
            lifetime ??= Lifetime.Static;

            _lifetime = lifetime;
            _lifetime.Register(this);

            Registry.Global.Register(this);
        }

        public bool IsDisposed = false;

        public void Dispose()
        {
            try // will never fail
            {
                lock (this)
                {
                    if (!IsDisposed) DoClean(); // TODO may be able to use Maybe.Lazy pattern
                    IsDisposed = true;
                    _lifetime.Deregister(this);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        ~Cleanable()
        {
            Dispose();
        }

        public abstract void DoClean();

        // TODO: the following should be in subclass "Service"
        public virtual string GetStatusSummary()
        {
            var vType = GetType();
            var text = vType.Name;
            return text;
        }

        public virtual IEnumerable<string> GetStatusDetail()
        {
            var uptime = DateTime.UtcNow - CreatedAt;

            return new List<string>
            {
                ToString(),
                $"    - ID: {ID}",
                $"    - Uptime: {uptime}"
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


    public static class CleanableExtensions
    {
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