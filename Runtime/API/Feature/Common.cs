using System;
using System.Collections.Generic;
using System.Linq;
using MAVLinkAPI.Routing;
using MAVLinkAPI.Util;
using MAVLinkAPI.Util.NullSafety;
using MAVLinkAPI.Util.Resource;
using UnityEngine;

namespace MAVLinkAPI.API.Feature
{
    // TODO: how about making it a normal class with metrics?
    public static class Common
    {
        public static Reader<Quaternion> ReadAttitude(
            this Uplink uplink
        )
        {
            var getAttitudeQ = MAVFunction.On<MAVLink.mavlink_attitude_t>()
                .Select((_, msg) =>
                {
                    var data = msg.Data;

                    var q = UnityQuaternionExtensions.AeronauticFrame.FromEulerRadian(
                        -data.pitch, data.yaw, -data.roll
                    );

                    return q;
                });


            return uplink.Read(getAttitudeQ);
        }

        // experimental, supported only by ArduPilot 4.1.0+
        public static Reader<Quaternion> ReadAttitudeQ(
            this Uplink uplink
        )
        {
            // var backup = MAVFunction.On<MAVLink.mavlink_attitude_quaternion_cov_t>(); TODO: switch to this for health check
            var getAttitudeQ = MAVFunction.On<MAVLink.mavlink_attitude_quaternion_t>()
                .Select((_, msg) =>
                {
                    var data = msg.Data;

                    // receiving quaternion in WXYZ order, FRD frame when facing north (a.k.a NED frame) (right-hand)
                    // FRD = Fowward-Right-Down
                    // NED = North-East-Down
                    // see MAVLink common.xml

                    // converting to XYZW order

                    // var q = new Quaternion(data.q1, data.q2, data.q3, data.q4);
                    // var q = new Quaternion(
                    //     -data.q2, -data.q4, -data.q3, data.q1
                    // ); // chiral conversion
                    var q = UnityQuaternionExtensions.AeronauticFrame.From(
                        data.q1, data.q2, data.q3, data.q4
                    );

                    return q;
                });


            return uplink.Read(getAttitudeQ);
        }

        public class NavigationFeed : RecurrentDaemon
        {
            public static NavigationFeed OfUplink(Lifetime lifetime, Uplink uplink)
            {
                var watchDog = Minimal.WatchDog(uplink);
                var attitudeReader = ReadAttitude(uplink);

                var result = new NavigationFeed(lifetime)
                {
                    WatchDog = watchDog,
                    AttitudeReader = attitudeReader
                };

                return result;
            }

            private NavigationFeed(Lifetime lifetime) : base(lifetime)
            {
            }

            public Reader<Message<MAVLink.mavlink_heartbeat_t>> WatchDog { get; init; }
            public Reader<Quaternion> AttitudeReader { get; init; }


            // TODO: need latency
            public Atomic<DateTime> LastHeartBeat = new(DateTime.MinValue);

            // TODO: need covariance
            public Atomic<Quaternion> LastAttitude = new(Quaternion.identity);

            private Maybe<Reader<object>> _updater;

            public Reader<object> Updater => _updater.Lazy(() =>
            {
                return WatchDog
                    .SelectMany((_, v) =>
                        {
                            LastHeartBeat.Value = v.RxTime;
                            return new List<object> { };
                        }
                    )
                    .Union(
                        AttitudeReader.SelectMany((_, v) =>
                        {
                            LastAttitude.Value = v;
                            return new List<object> { };
                        })
                    );
            });


            protected override void Iterate()
            {
                Updater.Drain();
            }

            public override void DoClean()
            {
                base.DoClean();
                foreach (var uplink in Updater.Sources.Keys) uplink.Dispose();
            }

            public override IEnumerable<string> GetStatusDetail()
            {
                var list = new List<string>
                {
                    $"    - heartbeat count : {LastHeartBeat.UpdateCount}",
                    $"    - attitude count : {LastAttitude.UpdateCount}"
                };

                return list.Union(base.GetStatusDetail());
            }
        }
    }
}