using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MAVLinkAPI.Routing;
using MAVLinkAPI.Util;
using Unity.VisualScripting;

namespace MAVLinkAPI.API.Feature
{
    public static class Minimal
    {
        public static MAVLink.mavlink_heartbeat_t HeartbeatFromHere =>
            new() // this should be sent regardless of received heartbeat
            {
                custom_mode = 0, // not sure how to use this
                mavlink_version = 3,
                type = (byte)MAVLink.MAV_TYPE.GCS,
                autopilot = (byte)MAVLink.MAV_AUTOPILOT.INVALID,
                base_mode = 0
            };

        public static Reader<object> WatchDog(
            this Uplink uplink,
            bool requireReceivedBytes = true,
            bool requireHeartbeat = true
        )
        {
            // this will return an empty reader that respond to heartbeat and request target to send all data
            // will fail if heartbeat is not received within 2 seconds

            var fn = MAVFunction
                .On<MAVLink.mavlink_heartbeat_t>()
                .SelectMany((_, msg) =>
                    {
                        var sender = msg.Sender;

                        // var heartbeatBack = ctx.Msg.Data;
                        var ack = HeartbeatFromHere;

                        // TODO: too frequent, should only send once
                        var requestAll = new MAVLink.mavlink_request_data_stream_t
                        {
                            req_message_rate = 2,
                            req_stream_id = (byte)MAVLink.MAV_DATA_STREAM.ALL,
                            start_stop = 1,
                            target_component = sender.ComponentID,
                            target_system = sender.SystemID
                        };

                        uplink.WriteData(ack);
                        uplink.WriteData(requestAll);


                        return new List<object>();
                    }
                );

            var reader = uplink.Read(fn);

            Retry.UpTo(12).With(TimeSpan.Zero).FixedInterval
                .Run((_, _) =>
                    {
                        uplink.WriteData(HeartbeatFromHere);

                        Thread.Sleep(200); // wait for a while before collecting

                        if (requireReceivedBytes)
                        {
                            var minReadBytes = 8;

                            //sanity check, port is deemed unusable if it doesn't receive any data
                            Retry.UpTo(24).With(TimeSpan.FromSeconds(0.2)).FixedInterval
                                .Run((_, tt) =>
                                    {
                                        if (uplink.BytesToRead >= minReadBytes)
                                        {
                                            // Debug.Log(
                                            //     $"Start reading serial port {Port.PortName} (with baud rate {Port.BaudRate}), received {Port.BytesToRead} byte(s)");
                                        }
                                        else
                                        {
                                            throw new TimeoutException(
                                                $"{uplink} only received {uplink.BytesToRead} byte(s) after {tt.TotalSeconds} seconds\n"
                                                + $" Expecting at least {minReadBytes} bytes");
                                        }
                                    }
                                );
                        }

                        if (requireHeartbeat)
                        {
                            reader.Drain();

                            if (reader.Sources.Keys.Sum(uplink =>
                                    uplink.Metrics.Counters.Get<MAVLink.mavlink_heartbeat_t>().ValueOrDefault.Value) <=
                                0)
                                throw new InvalidConnectionException(
                                    $"No heartbeat received");
                        }
                    }
                );

            return reader;
        }
    }
}