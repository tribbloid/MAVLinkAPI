using System;
using System.Collections.Generic;
using System.Linq;
using MAVLinkAPI.Util.NullSafety;
using UnityEngine;

namespace MAVLinkAPI.UI
{
    [Serializable]
    public class SerializedLookup<TK, TV> : Dictionary<TK, TV>
    {
        [Serializable]
        public class Pair
        {
            [Required] public TK key;
            [Required] public TV value;
        }

        [SerializeField] public List<Pair> pairs = new();

        private Maybe<Dictionary<TK, TV>?> _lookup;

        public Dictionary<TK, TV>? Dictionary => _lookup.Lazy(() =>
            pairs.ToDictionary(x => x.key, x => x.value)
        );
    }
}