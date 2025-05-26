// using System.Linq;
// using System.Threading;
// using MAVLinkAPI.Pose;
// using MAVLinkAPI.Routing;
// using MAVLinkAPI.Util;
// using NUnit.Framework;
// using UnityEngine;
//
// namespace MAVLinkAPI.Tests.Pose
// {
//     [Ignore("need SITL")]
//     public class MAVPoseFeedSpec
//     {
//         [Test]
//         public void ConnectAndRead10()
//         {
//             var feed = new MAVPoseFeed(IOStream.ArgsT.UDPLocalDefault);
//
//             var counter = new AtomicInt();
//
//             for (var i = 0; i < 1000; i++)
//             {
//                 var qs = feed.Reader.Drain();
//
//                 if (qs.Count > 0) counter.Increment();
//
//                 Debug.Log($"Quaternion: " +
//                           $"{qs.Aggregate("", (acc, q) => acc + q + "\n")}");
//
//                 if (counter.Get() > 10) break;
//             }
//         }
//
//         [Test]
//         public void ConnectAndUpdate()
//         {
//             var feed = new MAVPoseFeed(IOStream.ArgsT.UDPLocalDefault);
//             feed.StartUpdate();
//
//             var counter = new AtomicInt();
//
//             for (var i = 0; i < 1000; i++)
//             {
//                 Thread.Sleep(100);
//                 var qs = feed.UpdaterDaemon!.Attitude;
//
//                 if (qs != Quaternion.identity) counter.Increment();
//
//                 Debug.Log($"Quaternion: {qs}");
//
//                 if (counter.Get() > 10) break;
//             }
//         }
//     }
// }

