#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MAVLinkAPI.API;
using MAVLinkAPI.Util;
using MAVLinkAPI.Util.NullSafety;
using MAVLinkAPI.Util.Resource;

namespace MAVLinkAPI.Routing
{
    public abstract class Uplink : Cleanable
    {
        protected Uplink(
            Lifetime? lifetime = null
        ) : base(lifetime)
        {
        }

        // only has 1 impl, so this interface is optional


        public abstract int BytesToRead { get; }

        public abstract IEnumerable<MAVLink.MAVLinkMessage> RawReadSource { get; }

        public abstract void WriteData<T>(T data) where T : struct;

        public record MetricT(
            Uplink Outer,
            AtomicLong PacketCount,
            IDIndexed<AtomicLong> Histogram
        );

        private Maybe<MetricT> _metric; // fuck C# verbosity
        public MetricT Metric => _metric.Lazy(() => new MetricT(this, new AtomicLong(), new IDIndexed<AtomicLong>()));

        public readonly List<object> SubscribedReaders = new();
        // having multiple readers polling at the same time is dangerous, but we won't give a warning or error
        //  the burden is on the user

        public Reader<T> Read<T>(MAVFunction<T> mavFunction)
        {
            var reader = new Reader<T>(this, mavFunction);
            SubscribedReaders.Add(reader);
            return reader;
        }

        public Reader<Message<T>> On<T>() where T : struct
        {
            return Read(MAVFunction.On<T>());
        }

        // Mock Uplink that can provide a stream of messages for testing
        public new class Dummy : Uplink
        {
            private readonly IEnumerable<MAVLink.MAVLinkMessage> _messages;

            public Dummy(IEnumerable<MAVLink.MAVLinkMessage> messages)
            {
                _messages = messages;
            }


            public Dummy() : this(new List<MAVLink.MAVLinkMessage>())
            {
            }

            public override int BytesToRead => _messages.Any() ? 100 : 0;

            public override IEnumerable<MAVLink.MAVLinkMessage> RawReadSource => _messages;

            public override void WriteData<T>(T data) where T : struct
            {
                // dummy
            }

            public override void DoClean()
            {
                // dummy
            }
        }
    }
}