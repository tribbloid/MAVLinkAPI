#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MAVLinkAPI.Routing;
using MAVLinkAPI.Util.NullSafety;

namespace MAVLinkAPI.API
{
    /**
     * A.k.a subscription
     */
    public class Reader<T>
    {
        public readonly IDictionary<Uplink, MAVFunction<T>> Sources;

        public Reader(IDictionary<Uplink, MAVFunction<T>> sources)
        {
            Sources = sources;
        }

        public Reader(Routing.Uplink uplink, MAVFunction<T> mavFunction) : this(
            new Dictionary<Routing.Uplink, MAVFunction<T>> { { uplink, mavFunction } })
        {
        }

        record SubReader(KeyValuePair<Uplink, MAVFunction<T>> Pair)
        {
            public IEnumerable<List<T>> ByMessage_Mk()
            {
                return Pair.Key.RawReadSource.Select(message => Pair.Value.Process(message));
            }


            public int BytesToRead => Pair.Key.BytesToRead;

            public List<T> Drain(int leftover = 8)
            {
                var list = new List<T>();

                using (var itr = ByMessage_Mk().GetEnumerator())
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
        }

        private Maybe<IEnumerable<List<T>>> _byMessage;
        public IEnumerable<List<T>> ByMessage => _byMessage.Lazy(ByMessage_Mk);

        private IEnumerable<List<T>> ByMessage_Mk()
        {
            return Sources.SelectMany(pair => new SubReader(pair).ByMessage_Mk());
        }

        private Maybe<IEnumerable<T>> _byOutput;
        public IEnumerable<T> ByOutput => _byOutput.Lazy(ByOutput_Mk);

        private IEnumerable<T> ByOutput_Mk()
        {
            return ByMessage.SelectMany(vs => vs);
        }

        public Reader<T2> Discard<T2>()
        {
            var newSources = Sources.ToDictionary(
                pair => pair.Key,
                pair => (MAVFunction<T2>)pair.Value.SelectMany<T2>((m, v) => new List<T2>()));
            return new Reader<T2>(newSources);
        }

        public int BytesToRead => Sources.Keys.Sum(u => u.BytesToRead);

        public List<T> Drain(int leftover = 8)
        {
            return Sources.SelectMany(pair => new SubReader(pair).Drain()).ToList();
        }

        // TODO: do we need ChunkSelectMany<T>: List<T> => List<T2>?

        public Reader<T2> SelectMany<T2>(Func<MAVLink.MAVLinkMessage, T, List<T2>> fn)
        {
            var newSources = Sources.ToDictionary(
                pair => pair.Key,
                pair => (MAVFunction<T2>)pair.Value.SelectMany(fn));
            return new Reader<T2>(newSources);
        }

        public Reader<T2> Select<T2>(Func<MAVLink.MAVLinkMessage, T, T2> fn)
        {
            var newSources = Sources.ToDictionary(
                pair => pair.Key,
                pair => (MAVFunction<T2>)pair.Value.Select(fn));
            return new Reader<T2>(newSources);
        }

        // public Reader<object> ForEach(Action<MAVLink.MAVLinkMessage, T> ac)
        // { TODO:// need a better name
        //     var newSources = Sources.ToDictionary(
        //         pair => pair.Key,
        //         pair => (MAVFunction<object>)pair.Value.SelectMany((x, y) => ac(x, y); ));
        //     return new Reader<object>(newSources);
        // }

        public Reader<T> OrElse(Reader<T> that)
        {
            return Combine(that, (f1, f2) => f1.OrElse(f2));
        }

        public Reader<T> Union(Reader<T> that)
        {
            return Combine(that, (f1, f2) => f1.Union(f2));
        }

        private Reader<T> Combine(Reader<T> that, Func<MAVFunction<T>, MAVFunction<T>, MAVFunction<T>> combineFn)
        {
            var newSources = new Dictionary<Routing.Uplink, MAVFunction<T>>(this.Sources);
            foreach (var (uplink, mavFunction) in that.Sources)
            {
                if (newSources.TryGetValue(uplink, out var existingMavFunction))
                {
                    newSources[uplink] = combineFn(existingMavFunction, mavFunction);
                }
                else
                {
                    newSources[uplink] = mavFunction;
                }
            }

            return new Reader<T>(newSources);
        }
    }


    public static class ReaderExtensions
    {
        public static Reader<T> Upcast<T, T1>(this Reader<T1> reader) where T1 : T
        {
            var newSources = reader.Sources.ToDictionary(
                pair => pair.Key,
                pair => (MAVFunction<T>)pair.Value.Upcast<T, T1>());
            return new Reader<T>(newSources);
        }
    }
}