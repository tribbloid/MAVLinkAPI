using UnityEngine;

namespace MAVLinkAPI.Util.Resource
{
    public class LifetimeController : MonoBehaviour
    {
        public Lifetime lifetime = new();

        public Lifetime ManagedLifetime => lifetime;

        // OnDestroy will dispose lifetime
        private void OnDestroy()
        {
            lifetime.Dispose();
        }
    }
}