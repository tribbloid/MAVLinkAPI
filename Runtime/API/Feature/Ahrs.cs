using System.Collections.Generic;
using MAVLinkAPI.Routing;
using MAVLinkAPI.Util;
using MAVLinkAPI.Util.NullSafety;
using MAVLinkAPI.Util.Resource;
using UnityEngine;

namespace MAVLinkAPI.API.Feature
{
    public static class Ahrs
    {
        public static Reader<Quaternion> Attitude(
            this Routing.Uplink uplink
        )
        {
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

        public class Daemon : RecurrentDaemon
        {
            public Reader<object> WatchDog;
            public Reader<Quaternion> AttitudeReader;

            public Routing.Uplink Uplink;

            private Maybe<Reader<object>> _compoundReader = new();

            public Daemon(Lifetime lifetime, Routing.Uplink uplink) : base(lifetime)
            {
                Uplink = uplink;
                WatchDog = Minimal.WatchDog(uplink);
                AttitudeReader = Ahrs.Attitude(uplink);
            }

            public Quaternion Attitude = Quaternion.identity;

            public Reader<object> CompoundReader => _compoundReader.Lazy(() =>
            {
                return WatchDog.Union(
                    AttitudeReader.SelectMany((_, v) =>
                    {
                        Attitude = v;
                        return new List<object> { };
                    })
                );
            });


            protected override void Iterate()
            {
                CompoundReader.Drain();
            }

            protected override void DoClean()
            {
                base.DoClean();
                Uplink.Dispose();
            }
        }

        // public void StartUpdate()
        // {
        //     var reader = Reader;
        //
        //     lock (this)
        //     {
        //         if (UpdaterDaemon == null)
        //         {
        //             var daemon = new Daemon
        //             {
        //                 AttitudeReader = reader
        //             };
        //             daemon.Start();
        //             UpdaterDaemon = daemon;
        //         }
        //     }
        // }
        //
        // public void StopUpdate()
        // {
        //     lock (this)
        //     {
        //         if (UpdaterDaemon != null)
        //         {
        //             UpdaterDaemon.Dispose();
        //             UpdaterDaemon = null;
        //         }
        //     }
        // }
    }
}