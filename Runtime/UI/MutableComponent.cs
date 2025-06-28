using System;
using MAVLinkAPI.Util.NullSafety;
using UnityEngine;

namespace MAVLinkAPI.UI
{
    [Serializable]
    public class MutableComponent<T> where T : Component
    {
        [Required] public volatile T mutable;

        private struct OldLocation
        {
            public Transform Parent;
            public int SiblingIndex;
        }

        private Maybe<OldLocation> _stats;

        private OldLocation Stats => _stats.Lazy(() => new OldLocation
        {
            Parent = mutable.gameObject.transform.parent,
            SiblingIndex = mutable.gameObject.transform.GetSiblingIndex()
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
            newTransform.SetParent(Stats.Parent);
            newTransform.SetSiblingIndex(Stats.SiblingIndex);

            mutable = newInstance;
            return newInstance;
        }
    }
}