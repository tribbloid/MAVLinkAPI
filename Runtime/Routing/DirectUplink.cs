#nullable enable
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using MAVLinkAPI.API;
using MAVLinkAPI.Util;
using MAVLinkAPI.Util.NullSafety;
using MAVLinkAPI.Util.Resource;
using UnityEngine;
using Component = MAVLinkAPI.API.Component;

namespace MAVLinkAPI.Routing
{
    public class DirectUplink : Uplink
    {
        public readonly IOStream IO;
        public readonly Component ThisComponent;

        public readonly MAVLink.MavlinkParse Mavlink = new();

        public DirectUplink(
            IOStream io,
            Component? thisComponent = null,
            Lifetime? lifetime = null
        ) : base(lifetime)
        {
            IO = io;
            ThisComponent = thisComponent ?? Component.Gcs0;

            lock (Registry.GlobalAccessLock)
            {
                var peerClosed = 0;

                // close others with same name
                var peers = this.Peers().ToList();
                if (peers.Count > 0)
                    Debug.LogWarning(
                        $"found {peers.Count} peer(s) among {Registry.Global.Managed.Count} object(s)");

                foreach (var peer in peers)
                    if (peer.IO.Args.URIString == IO.Args.URIString)
                    {
                        peer.Dispose();
                        peerClosed += 1;
                    }

                if (peerClosed > 0)
                {
                    Debug.LogWarning($"{peerClosed} peer(s) with name {IO.Args.URIString} are closed");
                    Thread.Sleep(1000);
                }
            }
        }

        public override void DoClean()
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
                    return Result();

                    IEnumerable<MAVLink.MAVLinkMessage> Result()
                    {
                        while (IO.IsOpen)
                        {
                            MAVLink.MAVLinkMessage result;
                            lock (IO.ReadLock)
                            {
                                if (!IO.IsOpen) break;
                                result = Mavlink.ReadPacket(IO.BaseStream);
                            }

                            if (result == null)
                            {
                                // var pending = Port.BytesToRead;
                                // Debug.Log($"unknown packet, {pending} byte(s) left");
                            }
                            else
                            {
                                Metric.PacketCount.Value = Mavlink.packetcount;
                                var counter = Metric.Histogram.Get(result.msgid)
                                    .ValueOrInsert(() => new AtomicLong());
                                counter.Increment();

                                // Debug.Log($"received packet, info={TypeLookup.Global.ByID.GetValueOrDefault(result.msgid)}");
                                yield return result;
                            }
                        }
                    }
                }
            );


        public override IEnumerable<string> GetStatusDetail()
        {
            var list = new List<string>
            {
                $"    - buffer pressure : {IO.Metric_BufferPressure}",
                $"    - packet count : {Metric.PacketCount.Value}"
            };

            return list.Union(base.GetStatusDetail());
        }
    }
}