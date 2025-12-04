#nullable enable
using System.Collections.Generic;
using MAVLinkAPI.API;
using NUnit.Framework;

namespace MAVLinkAPI.Tests.API.Pipes
{
    [TestFixture]
    public class OnTSpec
    {
        [Test]
        public void Process_WrapsRawMessageAsRxMessage()
        {
            var on = MAVFunction.On<MAVLink.mavlink_heartbeat_t>();
            var message = MAVFunction.MockHeartbeat();

            var result = on.Process(message);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));

            var rx = result[0];
            Assert.That(rx.Raw.msgid, Is.EqualTo(message.msgid));
            Assert.That(rx.Data.type, Is.EqualTo((byte)MAVLink.MAV_TYPE.QUADROTOR));
            Assert.That(rx.Sender.SystemID, Is.EqualTo(message.sysid));
        }

        [Test]
        public void Process_RespectsPreviousFunction()
        {
            var prev = MAVFunction.Raw.SelectMany((m, raw) =>
                new List<MAVLink.MAVLinkMessage> { raw, raw });
            var on = MAVFunction.On<MAVLink.mavlink_heartbeat_t>(prev);
            var message = MAVFunction.MockHeartbeat();

            var result = on.Process(message);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
        }
    }
}