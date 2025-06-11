#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MAVLinkAPI.Util;

namespace MAVLinkAPI.API
{
    /**
     * A.k.a subscription
     */
    public struct Reader<T>
    {
        public MAVConnection Active;

        public MAVFunction<T> MAVFunction;

        private IEnumerable<List<T>>? _byMessage;

        public IEnumerable<List<T>> ByMessage =>
            LazyHelper.EnsureInitialized(ref _byMessage,
                _byMessage_Mk); // LazyInitializer.EnsureInitialized(ref _byMessage, _byMessage_Mk);

        private IEnumerable<List<T>> _byMessage_Mk()
        {
            foreach (var message in Active.RawReadSource)
            {
                var values = MAVFunction.Process(message);

                yield return values;
            }
        }

        public bool HasMore => ByMessage.Any();

        private IEnumerable<T> _byOutput;

        public IEnumerable<T> ByOutput => LazyInitializer.EnsureInitialized(ref _byOutput, _byOutput_Mk);

        private IEnumerable<T> _byOutput_Mk()
        {
            return ByMessage.SelectMany(vs => vs);
        }

        public List<T> Drain(int leftover = 8)
        {
            var list = new List<T>();

            using (var itr = ByMessage.GetEnumerator())
            {
                while (Active.IO.BytesToRead > leftover && itr.MoveNext())
                {
                    var current = itr.Current;
                    if (current != null)
                        // Debug.Log("Draining, " + Active.Port.BytesToRead + " bytes left");
                        list.AddRange(current);
                }
            }

            return list;
        }
    }
}