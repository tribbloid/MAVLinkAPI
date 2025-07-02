#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MAVLinkAPI.Ext;
using MAVLinkAPI.Util.NullSafety;

namespace MAVLinkAPI.API
{
    public static class MAVFunction
    {
        public class RawT : MAVFunction<MAVLink.MAVLinkMessage>
        {
            protected override IDIndexed<CaseFn> Topics_Mk()
            {
                return new IDIndexed<CaseFn>(); // no topic
            }

            protected override CaseFn OtherCase => m => new List<MAVLink.MAVLinkMessage> { m };
        }

        public static readonly RawT Raw = new();

        public class OnT<T> : MAVFunction<Message<T>> where T : struct
        {
            public readonly MAVFunction<MAVLink.MAVLinkMessage> Prev;

            public OnT(MAVFunction<MAVLink.MAVLinkMessage> prev)
            {
                Prev = prev;
            }

            protected override IDIndexed<CaseFn> Topics_Mk()
            {
                var result = new IDIndexed<CaseFn>();
                CaseFn topic = message =>
                {
                    var ms = Prev.Process(message);
                    var res = ms.SelectMany(x => new List<Message<T>> { Message<T>.FromRaw(x) }).ToList();

                    return res;
                };
                result.Get<T>().Value = topic;
                return result;
            }
        }

        public static OnT<T> On<T>(MAVFunction<MAVLink.MAVLinkMessage>? prev = null)
            where T : struct // TODO: this should be a shortcut on Uplink
        {
            prev ??= Raw;

            return new OnT<T>(prev);
        }
    }


    public abstract class MAVFunction<T>
    {
        public delegate List<T>? CaseFn(MAVLink.MAVLinkMessage message);

        private Maybe<IDIndexed<CaseFn>> _topics;

        private IDIndexed<CaseFn> Topics => _topics.Lazy(Topics_Mk);

        protected abstract IDIndexed<CaseFn> Topics_Mk();

        protected virtual CaseFn OtherCase => _ => null;
        // theoretically this will be interned to avoid multiple initialization

        public List<T>? ProcessOrNull(MAVLink.MAVLinkMessage message)
        {
            return Topics.Get(message.msgid).ValueOr(OtherCase)(message);
        }

        public List<T> Process(MAVLink.MAVLinkMessage message)
        {
            return ProcessOrNull(message) ?? new List<T>();
        }

        public static implicit operator Func<MAVLink.MAVLinkMessage, List<T>?>(MAVFunction<T> function)
        {
            return function.Process;
        }

        public abstract class BothT : MAVFunction<T>
        {
            public MAVFunction<T> Left = null!; // TODO: T in left and right can have different subtypes
            public MAVFunction<T> Right = null!;
        }

        public class UnionT : BothT
        {
            protected override IDIndexed<CaseFn> Topics_Mk()
            {
                var merged = Left.Topics.Index.Merge(
                    Right.Topics.Index,
                    (ll, rr) => input => ll(input).UnionNullSafe(rr(input))?.ToList());

                return new IDIndexed<CaseFn>(merged);
            }

            protected override CaseFn OtherCase => m =>
                Left.OtherCase(m).UnionNullSafe(Right.OtherCase(m))?.ToList();
        }

        public UnionT Union(MAVFunction<T> that)
        {
            return new UnionT
            {
                Left = this,
                Right = that
            };
        }

        public class OrElseT : BothT
        {
            protected override IDIndexed<CaseFn> Topics_Mk()
            {
                var merged = Left.Topics.Index.Merge(
                    Right.Topics.Index,
                    (ll, rr) => input => ll(input) ?? rr(input));

                return new IDIndexed<CaseFn>(merged);
            }


            protected override CaseFn OtherCase => m => Left.OtherCase(m) ?? Right.OtherCase(m);
        }

        public OrElseT OrElse(MAVFunction<T> that)
        {
            return new OrElseT
            {
                Left = this,
                Right = that
            };
        }

        public class CutElimination<T2> : MAVFunction<T2>
        {
            public MAVFunction<T> Prev = null!;
            public Func<MAVLink.MAVLinkMessage, T, List<T2>> Fn = null!;

            protected override IDIndexed<CaseFn> Topics_Mk()
            {
                var oldTopics = Prev.Topics.Index;

                var newTopics = oldTopics.ToDictionary(
                    kv => kv.Key,
                    kv =>
                    {
                        CaseFn topic = ii =>
                        {
                            var prevV = kv.Value(ii);
                            if (prevV == null) return null;

                            var result = prevV.SelectMany(x => Fn(ii, x)
                            ).ToList();

                            return result;
                        };
                        return topic;
                    }
                );

                return new IDIndexed<CaseFn>(newTopics);
            }

            protected override CaseFn OtherCase => m =>
            {
                var prevV = Prev.OtherCase(m);
                if (prevV == null) return null;

                return prevV.SelectMany(v =>
                    Fn(m, v)
                ).ToList();
            };
        }

        public CutElimination<T2> SelectMany<T2>(Func<MAVLink.MAVLinkMessage, T, List<T2>> fn)
        {
            return new CutElimination<T2>
            {
                Prev = this,
                Fn = fn
            };
        }

        public CutElimination<T2> Select<T2>(Func<MAVLink.MAVLinkMessage, T, T2> fn)
        {
            return SelectMany((ii, x) => new List<T2> { fn(ii, x) });
        }
    }

    public static class ProcessorExtensions
    {
        // TODO: will be redundant if MAVFunction<T> => IMAVFunction<out T>
        public static MAVFunction<T> Upcast<T, T1>(this MAVFunction<T1> left) where T1 : T
        {
            return left.Select((_, t) => (T)t);
        }
    }
}