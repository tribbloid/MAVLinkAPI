#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MAVLinkAPI.Routing;
using MAVLinkAPI.Util;

namespace MAVLinkAPI.API
{
    /**
     * A.k.a subscription
     */
    public struct Reader<T>
    {
        public Uplink Uplink;
        public MAVFunction<T> MAVFunction;

        private IEnumerable<List<T>>? _byMessage;

        public IEnumerable<List<T>> ByMessage =>
            LazyHelper.EnsureInitialized(ref _byMessage, MkByMessage);

        private IEnumerable<List<T>> MkByMessage()
        {
            foreach (var message in Uplink.RawReadSource)
            {
                var values = MAVFunction.Process(message);

                yield return values;
            }
        }

        public bool HasMore => ByMessage.Any();

        private IEnumerable<T> _byOutput;

        public IEnumerable<T> ByOutput => LazyInitializer.EnsureInitialized(ref _byOutput, MkByOutput);

        private IEnumerable<T> MkByOutput()
        {
            return ByMessage.SelectMany(vs => vs);
        }

        public Reader<T2> Discard<T2>()
        {
            var newFn = MAVFunction.SelectMany<T2>((m, v) => new List<T2>());
            return new Reader<T2> { Uplink = Uplink, MAVFunction = newFn };
        }

        public int BytesToRead => Uplink.IO.BytesToRead;

        public List<T> Drain(int leftover = 8)
        {
            var list = new List<T>();

            using (var itr = ByMessage.GetEnumerator())
            {
                while (BytesToRead > leftover && itr.MoveNext())
                {
                    var current = itr.Current;
                    if (current != null)
                        list.AddRange(current);
                }
            }

            return list;
        }

        // TODO: do we need ChunkSelectMany<T>: List<T> => List<T2>?

        public Reader<T2> SelectMany<T2>(Func<MAVLink.MAVLinkMessage, T, List<T2>> fn)
        {
            var newFn = MAVFunction.SelectMany(fn);
            return new Reader<T2> { Uplink = Uplink, MAVFunction = newFn };
        }

        public Reader<T2> Select<T2>(Func<MAVLink.MAVLinkMessage, T, T2> fn)
        {
            var newFn = MAVFunction.Select(fn);
            return new Reader<T2> { Uplink = Uplink, MAVFunction = newFn };
        }


        public Reader<T> OrElse(Reader<T> that)
        {
            var newFn = MAVFunction.OrElse(that.MAVFunction);
            return new Reader<T> { Uplink = Uplink, MAVFunction = newFn };
        }

        public Reader<T> Union(Reader<T> that)
        {
            var newFn = MAVFunction.Union(that.MAVFunction);
            return new Reader<T> { Uplink = Uplink, MAVFunction = newFn };
        }
    }


    public static class ReaderExtensions
    {
        public static Reader<T> Upcast<T, T1>(this Reader<T1> reader) where T1 : T
        {
            var newFn = reader.MAVFunction.Upcast<T, T1>();
            return new Reader<T> { Uplink = reader.Uplink, MAVFunction = newFn };
        }
    }
}