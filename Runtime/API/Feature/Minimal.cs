using System;
using System.Threading;
using MAVLinkAPI.Routing;
using MAVLinkAPI.Util;
using Unity.VisualScripting;

namespace MAVLinkAPI.API.Feature
{
    public static class Minimal
    {
        public record ConstT
        {
            public readonly MAVLink.mavlink_heartbeat_t Ack =
                new() // this should be sent regardless of received heartbeat
                {
                    custom_mode = 0, // not sure how to use this
                    mavlink_version = 2,
                    type = (byte)MAVLink.MAV_TYPE.GCS,
                    autopilot = (byte)MAVLink.MAV_AUTOPILOT.INVALID,
                    base_mode = 0
                };
        }

        public static ConstT Const = new();

        public static Reader<RxMessage<MAVLink.mavlink_heartbeat_t>> WatchDog(
            this Uplink uplink,
            bool requireReceivedBytes = true,
            bool requireHeartbeat = true
        )
        {
            // this will return an empty reader that respond to heartbeat and request target to send all data
            // will fail if heartbeat is not received within 2 seconds

            var rawReader = uplink
                .On<MAVLink.mavlink_heartbeat_t>()
                .Select((_, msg) =>
                    {
                        // var heartbeatBack = ctx.Msg.Data;
                        uplink.WriteData(Const.Ack);

                        var sender = msg.Sender;

                        // TODO: too frequent, should only send once
                        //  req_message_rate is also too high for all stats, only need the relevant parts
                        //  also superseded by MAV_CMD_SET_MESSAGE_INTERVAL
                        var requestStream = new MAVLink.mavlink_request_data_stream_t
                        {
                            req_message_rate = 25,
                            req_stream_id = (byte)MAVLink.MAV_DATA_STREAM.ALL,
                            start_stop = 1,
                            target_component = sender.ComponentID,
                            target_system = sender.SystemID
                        };
                        uplink.WriteData(requestStream);

                        // MAV_CMD_SET_MESSAGE_INTERVAL same as above, but not deprecated 
                        // var setInterval = new MAVLink.mavlink_command_long_t
                        // {
                        //     command = (ushort)MAVLink.MAV_CMD.SET_MESSAGE_INTERVAL,
                        //     confirmation = 0,
                        //     param1 = 2, // req_message_rate
                        //     param2 = (byte)MAVLink.MAV_DATA_STREAM.ALL, // req_stream_id
                        //     param3 = 1, // start_stop
                        //     target_system = sender.SystemID,
                        //     target_component = sender.ComponentID
                        // };
                        // // TODO: expecting mavlink_message_interval_t
                        // uplink.WriteData(setInterval);

                        return msg;
                    }
                );

            var count = new AtomicInt();

            Retry.UpTo(6).With(TimeSpan.FromSeconds(0.2)).FixedInterval
                .Run((_, _) =>
                    {
                        uplink.WriteData(Const.Ack);

                        Thread.Sleep(500); // wait for a while before collecting

                        if (requireReceivedBytes)
                        {
                            var minReadBytes = 8;

                            //sanity check, port is deemed unusable if it doesn't receive any data
                            Retry.UpTo(6).With(TimeSpan.FromSeconds(0.8)).FixedInterval
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
                            var rxTimeReader = rawReader.Select((_, v) => { return v.RxTime; });

                            count.Add(rxTimeReader.Drain().Count);

                            if (count.Value == 0)
                                throw new InvalidConnectionException($"No heartbeat received");
                        }
                    }
                );

            return rawReader;
        }
    }
}