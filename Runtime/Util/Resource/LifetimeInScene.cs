using UnityEngine;

namespace MAVLinkAPI.Util.Resource
{
    public class LifetimeInScene : MonoBehaviour
    {
        public Lifetime Lifetime = new();

        public Lifetime ManagedLifetime => Lifetime;

        // OnDestroy will dispose lifetime
        private void OnDestroy()
        {
            Lifetime.Dispose();
        }
    }
}