#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MAVLinkAPI.Ext;
using MAVLinkAPI.Util;
using MAVLinkAPI.Util.NullSafety;

namespace MAVLinkAPI.API
{
    public static class MAVFunction
    {
        public class OnT<T> : MAVFunction<Message<T>> where T : struct
        {
            protected override IDIndexed<Topic> Topics_Mk()
            {
                IDIndexed<Topic> result = new IDIndexed<Topic>();
                Topic topic = message => new List<Message<T>>
                {
                    Message<T>.FromRaw(message)
                };
                result.Get<T>().Value = topic;
                return result;
            }
        }

        public static OnT<T> On<T>() where T : struct
        {
            return new OnT<T>();
        }
    }


    public abstract class MAVFunction<T>
    {
        public delegate List<T> Topic(MAVLink.MAVLinkMessage message);

        public static readonly Topic TopicMissing = _ => new List<T>();

        private Maybe<IDIndexed<Topic>> _topics;

        private IDIndexed<Topic> Topics =>
            _topics.Lazy(Topics_Mk);

        protected abstract IDIndexed<Topic> Topics_Mk();


        public List<T> Process(MAVLink.MAVLinkMessage message)
        {
            return Topics.Get(message.msgid).ValueOr(TopicMissing)(message);
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
            protected override IDIndexed<Topic> Topics_Mk()
            {
                var merged = Left.Topics.Index.Merge(
                    Right.Topics.Index,
                    (ll, rr) => input =>
                    {
                        var before = new List<List<T>?> { ll(input), rr(input) };

                        return before.Where(x => x != null)
                            .Aggregate((l, r) => l.Union(r).ToList());
                    }
                );

                return new IDIndexed<Topic>(merged);
            }
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
            protected override IDIndexed<Topic> Topics_Mk()
            {
                var merged = Left.Topics.Index.Merge(
                    Right.Topics.Index,
                    (ll, rr) => input =>
                    {
                        var before = (ll(input), rr(input));

                        return before.Item1 ?? before.Item2;
                    }
                );

                return new IDIndexed<Topic>(merged);
            }
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

            protected override IDIndexed<Topic> Topics_Mk()
            {
                var oldTopics = Prev.Topics.Index;

                var newTopics = oldTopics.ToDictionary(
                    kv => kv.Key,
                    kv =>
                    {
                        Topic topic = ii =>
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

                return new IDIndexed<Topic>(newTopics);
            }
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
        public static MAVFunction<T> Upcast<T, T1>(this MAVFunction<T1> left) where T1 : T
        {
            return left.Select((_, t) => (T)t);
        }
    }
}