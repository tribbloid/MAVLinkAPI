#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MAVLinkAPI.Scripts.Ext;
using MAVLinkAPI.Scripts.Util;

namespace MAVLinkAPI.Scripts.API
{
    public static class MAVFunction
    {
        public class OnT<T> : MAVFunction<Message<T>> where T : struct
        {
            protected override Indexed<Topic> Topics_Mk()
            {
                Indexed<Topic?> result = Indexed<Topic>.Global();
                result.Get<T>().Value = message => new List<Message<T>>
                {
                    Message<T>.FromRaw(message)
                };
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
        public delegate List<T>? Topic(MAVLink.MAVLinkMessage message);

        public static readonly Topic? TopicMissing = _ => null;

        private Box<Indexed<Topic>>? _topics;

        private Indexed<Topic> Topics =>
            // Topics_Mk.BindToLazy(ref _topics); // TODO: oops, eta-expansion does't work.
            LazyHelper.EnsureInitialized(ref _topics, Topics_Mk);

        protected abstract Indexed<Topic> Topics_Mk();


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
            protected override Indexed<Topic> Topics_Mk()
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

                return Indexed<Topic>.Global(merged);
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
            protected override Indexed<Topic> Topics_Mk()
            {
                var merged = Left.Topics.Index.Merge(
                    Right.Topics.Index,
                    (ll, rr) => input =>
                    {
                        var before = (ll(input), rr(input));

                        return before.Item1 ?? before.Item2;
                    }
                );

                return Indexed<Topic>.Global(merged);
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

            protected override Indexed<Topic> Topics_Mk()
            {
                var oldTopics = Prev.Topics.Index;

                var newTopics = oldTopics.ToDictionary(
                    kv => kv.Key,
                    kv =>
                    {
                        Topic topic = (ii) =>
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

                return Indexed<Topic>.Global(newTopics);
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

        public Reader<T> LatchOn(MAVConnection mav)
        {
            return mav.Read(this);
        }
    }

    // public static class ProcessorExtensions
    // {
    //     public static Subscriber<T>? Add<T>(this Subscriber<T>? left, Subscriber<T>? right)
    //     {
    //         var result = (left, right).NullableReduce(
    //             (x, y) => new Subscriber<T>.UnionT { Left = x, Right = y }
    //         );
    //
    //         return result;
    //     }
    //
    //     public static Subscriber<T>? OrElse<T>(this Subscriber<T>? left, Subscriber<T>? right)
    //     {
    //         var result = (left, right).NullableReduce(
    //             (x, y) => new Subscriber<T>.OrElseT { Left = x, Right = y }
    //         );
    //
    //         return result;
    //     }
    // }
}