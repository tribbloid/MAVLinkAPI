using System;
using MAVLinkAPI.Util.NullSafety;
using UnityEngine;

namespace MAVLinkAPI.UI
{
    [Serializable]
    public class MutableComponent<T> where T : Component
    {
        [Required] public volatile T mutable;

        struct OldLocation
        {
            public Transform parent;
            public int siblingIndex;
        }

        private Maybe<OldLocation> _stats;

        private OldLocation Stats => _stats.Lazy(() => new OldLocation
        {
            parent = mutable.gameObject.transform.parent,
            siblingIndex = mutable.gameObject.transform.GetSiblingIndex()
        });

        public T CopyToReplace(T template)
        {
            var newInstance = UnityEngine.Object.Instantiate(template);

            return MoveToReplace(newInstance);
        }

        public T MoveToReplace(T newInstance)
        {
            var oldGameObject = mutable.gameObject;
            UnityEngine.Object.Destroy(oldGameObject);

            var newTransform = newInstance.transform;
            newTransform.SetParent(Stats.parent);
            newTransform.SetSiblingIndex(Stats.siblingIndex);

            mutable = newInstance;
            return newInstance;
        }
    }
}