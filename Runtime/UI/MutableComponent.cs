using System;
using MAVLinkAPI.Util.NullSafety;
using UnityEngine;

namespace MAVLinkAPI.UI
{
    [Serializable]
    public class MutableComponent<T> where T : Component
    {
        [Required] public T mutable;

        private struct OldStatsT
        {
            public Transform Parent;
            public int SiblingIndex;
            public Vector3 LocalPosition;
            public Vector3 LocalRotation;
            public Vector3 LocalScale;
        }

        private Maybe<OldStatsT> _stats;

        private OldStatsT OldStats => _stats.Lazy(() =>
        {
            var t = mutable.gameObject.transform;
            return new OldStatsT
            {
                Parent = t.parent,
                SiblingIndex = t.GetSiblingIndex(),
                LocalPosition = t.localPosition,
                LocalRotation = t.localEulerAngles,
                LocalScale = t.localScale
            };
        });

        public T CopyToReplace(T template)
        {
            var newInstance = UnityEngine.Object.Instantiate(template);

            return MoveToReplace(newInstance);
        }

        public T MoveToReplace(T instance)
        {
            var oldStats = OldStats;
            UnityEngine.Object.Destroy(mutable.gameObject);

            var newTransform = instance.transform;
            newTransform.SetParent(oldStats.Parent);
            newTransform.SetSiblingIndex(oldStats.SiblingIndex);

            newTransform.localPosition = oldStats.LocalPosition;
            newTransform.localEulerAngles = oldStats.LocalRotation;
            newTransform.localScale = oldStats.LocalScale;


            mutable = instance;
            return instance;
        }
    }
}