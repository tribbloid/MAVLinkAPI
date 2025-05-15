using UnityEngine;

namespace MAVLinkAPI.Scripts.Util.Resource
{
    public class LifetimeController : MonoBehaviour
    {
        private Lifetime lifetime = new();

        public Lifetime ManagedLifetime => lifetime;

        // OnDestroy will dispose lifetime
        private void OnDestroy()
        {
            lifetime.Dispose();
        }
    }
}