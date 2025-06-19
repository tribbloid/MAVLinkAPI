#nullable enable
using System;
using System.Collections.Generic;
using MAVLinkAPI.API;
using MAVLinkAPI.Util;
using MAVLinkAPI.Util.NullSafety;
using MAVLinkAPI.Util.Resource;
using Component = MAVLinkAPI.API.Component;

namespace MAVLinkAPI.Routing
{
    public class DirectUplink : Uplink
    {
        public readonly IOStream IO;
        public readonly Component ThisComponent;

        public readonly MAVLink.MavlinkParse Mavlink = new();

        private DirectUplink(IOStream io, Component? thisComponent = null)
        {
            IO = io;
            ThisComponent = thisComponent ?? Component.Gcs0;
        }
        //
        // public Uplink(
        //     IOStream.ArgsT args,
        //     Lifetime? lifetime = null
        // ) : this(new IOStream(args, lifetime))
        // {
        // }

        public override void Dispose()
        {
            IO.Dispose();
        }


        public static IEnumerable<DirectUplink> Scan(
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

        public override void WriteData<T>(T data) where T : struct
        {
            var msg = ThisComponent.ToMessage(data);

            Write(msg);
        }

        public override int BytesToRead => IO.BytesToRead;

        private Maybe<IEnumerable<MAVLink.MAVLinkMessage>> _rawReadSource;

        public override IEnumerable<MAVLink.MAVLinkMessage> RawReadSource =>
            _rawReadSource.Lazy(() =>
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
    }
}