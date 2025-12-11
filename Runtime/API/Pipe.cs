#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MAVLinkAPI.API.Fn;
using MAVLinkAPI.Ext;
using MAVLinkAPI.Util.NullSafety;

namespace MAVLinkAPI.API
{
    public static class Pipe
    {
        /// <summary>
        /// Test utility method to create a mock MAVLink heartbeat message
        /// </summary>
        /// <returns>A MAVLinkMessage containing a heartbeat with quadrotor type configuration</returns>
        public static MAVLink.MAVLinkMessage MockHeartbeat()
        {
            var heartbeat = new MAVLink.mavlink_heartbeat_t
            {
                type = (byte)MAVLink.MAV_TYPE.QUADROTOR,
                autopilot = (byte)MAVLink.MAV_AUTOPILOT.GENERIC,
                base_mode = (byte)MAVLink.MAV_MODE_FLAG.MANUAL_INPUT_ENABLED,
                custom_mode = 0,
                system_status = (byte)MAVLink.MAV_STATE.STANDBY,
                mavlink_version = 3
            };

            var parser = new MAVLink.MavlinkParse();
            var packetBytes = parser.GenerateMAVLinkPacket20(MAVLink.MAVLINK_MSG_ID.HEARTBEAT, heartbeat);
            return new MAVLink.MAVLinkMessage(packetBytes);
        }

        public static readonly RawT Raw = new();

        public static OnT<T> On<T>(Pipe<MAVLink.MAVLinkMessage>? prev = null)
            where T : struct // TODO: this should be a shortcut on Uplink
        {
            prev ??= Raw;

            return new OnT<T>(prev);
        }
    }


    public abstract class Pipe<T>
    {
        public delegate List<T>? CaseFn(MAVLink.MAVLinkMessage message);

        private Maybe<IDIndexed<CaseFn>> _topics;

        public IDIndexed<CaseFn> Topics => _topics.Lazy(MkTopics);

        protected abstract IDIndexed<CaseFn> MkTopics();

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

        public static implicit operator Func<MAVLink.MAVLinkMessage, List<T>?>(Pipe<T> function)
        {
            return function.Process;
        }

        public abstract class LeftAndRight : Pipe<T>
        {
            public Pipe<T> Left = null!;
            public Pipe<T> Right = null!;
        }

        public class UnionT : LeftAndRight
        {
            protected override IDIndexed<CaseFn> MkTopics()
            {
                var merged = Left.Topics.Index.Merge(
                    Right.Topics.Index,
                    (ll, rr) => input => ll(input).UnionNullSafe(rr(input))?.ToList());

                return new IDIndexed<CaseFn>(merged);
            }

            protected override CaseFn OtherCase => m =>
                Left.OtherCase(m).UnionNullSafe(Right.OtherCase(m))?.ToList();
        }

        public UnionT Union(Pipe<T> that)
        {
            return new UnionT
            {
                Left = this,
                Right = that
            };
        }


        public class OrElseT : LeftAndRight
        {
            protected override IDIndexed<CaseFn> MkTopics()
            {
                var merged = Left.Topics.Index.Merge(
                    Right.Topics.Index,
                    (ll, rr) => input => ll(input) ?? rr(input));

                return new IDIndexed<CaseFn>(merged);
            }


            protected override CaseFn OtherCase => m => Left.OtherCase(m) ?? Right.OtherCase(m);
        }

        public OrElseT OrElse(Pipe<T> that)
        {
            return new OrElseT
            {
                Left = this,
                Right = that
            };
        }

        public class CutElimination<T2> : Pipe<T2>
        {
            public Pipe<T> Prev = null!;
            public Func<MAVLink.MAVLinkMessage, T, List<T2>> Fn = null!;

            protected override IDIndexed<CaseFn> MkTopics()
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
        // TODO: will be redundant if Pipe<T> => Pipe<out T>
        public static Pipe<T> Upcast<T, T1>(this Pipe<T1> left) where T1 : T
        {
            return left.Select((_, t) => (T)t);
        }
    }
}