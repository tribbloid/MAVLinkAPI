using MAVLinkAPI.Util.NullSafety;
using UnityEngine;

namespace MAVLinkAPI.Util.Resource
{
    public class LifetimeBinding : MonoBehaviour
    {
        protected Maybe<Lifetime> _lifetime;

        public virtual Lifetime Lifetime => _lifetime.Lazy(() => new Lifetime());

        // OnDestroy will dispose lifetime
        private void OnDestroy()
        {
            Lifetime.Dispose();
        }
    }
}