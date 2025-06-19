#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MAVLinkAPI.API;
using MAVLinkAPI.Util;

namespace MAVLinkAPI.Routing
{
    public abstract class Uplink : IDisposable
    {
        // only has 1 impl, so this interface is optional

        public abstract int BytesToRead { get; }

        public abstract IEnumerable<MAVLink.MAVLinkMessage> RawReadSource { get; }

        public abstract void WriteData<T>(T data) where T : struct;

        public readonly List<object> ExistingReaders = new();
        // having multiple readers polling at the same time is dangerous, but we won't give a warning or error
        //  the burden is on the user

        public Reader<T> Read<T>(MAVFunction<T> mavFunction)
        {
            var reader = new Reader<T>(this, mavFunction);
            ExistingReaders.Add(reader);
            return reader;
        }

        public class MetricsT
        {
            public IDIndexed<AtomicLong> Counters;

            public int BufferPressure; // pending data size in the buffer
        }

        public MetricsT Metrics { get; } = new()
        {
            Counters = new IDIndexed<AtomicLong>()
        };

        public abstract void Dispose();


        // Mock Uplink that can provide a stream of messages for testing
        public class Dummy : Uplink
        {
            private readonly List<MAVLink.MAVLinkMessage> _messages;

            public Dummy(List<MAVLink.MAVLinkMessage> messages)
            {
                _messages = messages;
            }


            public Dummy() : this(new List<MAVLink.MAVLinkMessage>())
            {
            }

            public override int BytesToRead => _messages.Any() ? 1 : 0;

            public override IEnumerable<MAVLink.MAVLinkMessage> RawReadSource => _messages;

            public override void WriteData<T>(T data) where T : struct
            {
                /* Do nothing */
            }

            public override void Dispose()
            {
                /* Do nothing */
            }
        }
    }
}