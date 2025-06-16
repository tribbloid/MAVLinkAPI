#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MAVLinkAPI.API;
using MAVLinkAPI.Util;
using MAVLinkAPI.Util.Resource;
using UnityEngine;
using Component = MAVLinkAPI.API.Component;

namespace MAVLinkAPI.Routing
{
    public class Uplink : IDisposable
    {
        public readonly IOStream IO;

        public readonly MAVLink.MavlinkParse Mavlink = new();
        public readonly Component ThisComponent = Component.Gcs0;

        private Uplink(IOStream io)
        {
            IO = io;
        }

        public Uplink(
            IOStream.ArgsT args,
            Lifetime? lifetime = null
        ) : this(new IOStream(args, lifetime))
        {
        }

        public void Dispose()
        {
            IO.Dispose();
        }


        public static IEnumerable<Uplink> Scan(
            IOStream.ArgsT args,
            Lifetime? lifetime = null
        )
        {
            throw new NotImplementedException("will be implemented later by CommsSerialScan");
        }


        public void Write<T>(Message<T> msg) where T : struct
        {
            // TODO: why not GenerateMAVLinkPacket10?
            var bytes = Mavlink.GenerateMAVLinkPacket20(
                msg.TypeID,
                msg.Data,
                sysid: ThisComponent.SystemID,
                compid: ThisComponent.ComponentID
            );

            IO.WriteBytes(bytes);
        }

        public void WriteData<T>(T data) where T : struct
        {
            var msg = ThisComponent.Send(data);

            Write(msg);
        }

        private IEnumerable<MAVLink.MAVLinkMessage>? _rawReadSource;

        public IEnumerable<MAVLink.MAVLinkMessage> RawReadSource =>
            LazyInitializer.EnsureInitialized(ref _rawReadSource, () =>
                {
                    return Get();

                    IEnumerable<MAVLink.MAVLinkMessage> Get()
                    {
                        while (IO.IsOpen)
                        {
                            MAVLink.MAVLinkMessage result;
                            lock (IO.ReadLock)
                            {
                                if (!IO.IsOpen) break;
                                result = Mavlink.ReadPacket(IO.BaseStream);
                                Metrics.BufferPressure = IO.BytesToRead;
                            }

                            if (result == null)
                            {
                                // var pending = Port.BytesToRead;
                                // Debug.Log($"unknown packet, {pending} byte(s) left");
                            }
                            else
                            {
                                var counter = Metrics.Counters.Get(result.msgid).ValueOrInsert(() => new AtomicLong());
                                counter.Increment();

                                // Debug.Log($"received packet, info={TypeLookup.Global.ByID.GetValueOrDefault(result.msgid)}");
                                yield return result;
                            }
                        }
                    }
                }
            );

        public readonly List<object> ExistingReaders = new();
        // having multiple readers polling at the same time is dangerous, but we won't give a warning or error
        //  the burden is on the user

        public Reader<T> Read<T>(MAVFunction<T> mavFunction)
        {
            var reader = new Reader<T> { Uplink = this, MAVFunction = mavFunction };
            ExistingReaders.Add(reader);
            return reader;
        }


        public struct MetricsT
        {
            public IDIndexed<AtomicLong?> Counters;

            public int BufferPressure; // pending data in the buffer
            public int LatencyMillis;

            // public double Health => // this will be computed from data
        }

        public MetricsT Metrics = new()
        {
            Counters = IDIndexed<AtomicLong>.Global()
        };
    }
}