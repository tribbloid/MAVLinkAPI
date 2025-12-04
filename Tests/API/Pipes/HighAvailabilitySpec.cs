using System;
using MAVLinkAPI.API;
using MAVLinkAPI.API.Fn;
using NUnit.Framework;

namespace MAVLinkAPI.Tests.API.Pipes
{
    [TestFixture]
    public class HighAvailabilitySpec
    {
        private class SilentFunction : MAVFunction<RxMessage<MAVLink.mavlink_heartbeat_t>>
        {
            protected override IDIndexed<CaseFn> MkTopics()
            {
                return new IDIndexed<CaseFn>();
            }

            protected override CaseFn OtherCase => _ => null;
        }

        private static HighAvailabilityT<MAVLink.mavlink_heartbeat_t> CreateHaLeftOnly()
        {
            var left = MAVFunction.On<MAVLink.mavlink_heartbeat_t>();
            var right = new SilentFunction();
            return new HighAvailabilityT<MAVLink.mavlink_heartbeat_t>
            {
                Left = left,
                Right = right
            };
        }

        private static HighAvailabilityT<MAVLink.mavlink_heartbeat_t> CreateHaBothChannels()
        {
            var left = MAVFunction.On<MAVLink.mavlink_heartbeat_t>();
            var right = MAVFunction.On<MAVLink.mavlink_heartbeat_t>();
            return new HighAvailabilityT<MAVLink.mavlink_heartbeat_t>
            {
                Left = left,
                Right = right
            };
        }

        [Test]
        public void FirstMessage_FromSingleChannel_IsForwarded_WithNoWarningsOrErrors()
        {
            var ha = CreateHaLeftOnly();
            var message = MAVFunction.MockHeartbeat();

            var result = ha.Process(message);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Count, Is.EqualTo(1));
            Assert.That(ha.WarningCount, Is.EqualTo(0));
            Assert.That(ha.ErrorCount, Is.EqualTo(0));
        }

        [Test]
        public void SecondMessage_FromSameChannel_IncrementsErrorCount_AndIsNotForwarded()
        {
            var ha = CreateHaLeftOnly();
            var baseTime = DateTime.UtcNow;

            var message1 = MAVFunction.MockHeartbeat();
            message1.rxtime = baseTime;

            var message2 = MAVFunction.MockHeartbeat();
            message2.rxtime = baseTime;

            var first = ha.Process(message1);
            var second = ha.Process(message2);

            Assert.That(first, Is.Not.Null);
            Assert.That(first!.Count, Is.EqualTo(1));
            Assert.That(second, Is.Not.Null);
            Assert.That(second!.Count, Is.EqualTo(0));
            Assert.That(ha.ErrorCount, Is.EqualTo(1));
            Assert.That(ha.WarningCount, Is.EqualTo(0));
        }

        [Test]
        public void Message_SeenOnBothChannels_IsVerifiedWithoutErrors()
        {
            var ha = CreateHaBothChannels();
            var message = MAVFunction.MockHeartbeat();

            var result = ha.Process(message);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Count, Is.EqualTo(1));
            Assert.That(ha.ErrorCount, Is.EqualTo(0));
            Assert.That(ha.WarningCount, Is.EqualTo(0));
        }

        [Test]
        public void UnverifiedMessage_BecomesStaleAfterStaleThreshold()
        {
            var ha = CreateHaLeftOnly();
            var baseTime = DateTime.UtcNow;

            var message1 = MAVFunction.MockHeartbeat();
            message1.rxtime = baseTime;

            var deltaTicks = (ha.LostThreshold - ha.StaleThreshold).Ticks / 2;
            var message2 = MAVFunction.MockHeartbeat();
            message2.rxtime = baseTime + ha.StaleThreshold + TimeSpan.FromTicks(deltaTicks);

            var first = ha.Process(message1);
            var second = ha.Process(message2);

            Assert.That(first, Is.Not.Null);
            Assert.That(first!.Count, Is.EqualTo(1));
            Assert.That(second, Is.Not.Null);
            Assert.That(second!.Count, Is.EqualTo(1));
            Assert.That(ha.WarningCount, Is.EqualTo(1));
            Assert.That(ha.ErrorCount, Is.EqualTo(0));
        }

        [Test]
        public void UnverifiedMessage_BecomesLostAfterLostThreshold()
        {
            var ha = CreateHaLeftOnly();
            var baseTime = DateTime.UtcNow;

            var message1 = MAVFunction.MockHeartbeat();
            message1.rxtime = baseTime;

            var message2 = MAVFunction.MockHeartbeat();
            message2.rxtime = baseTime + ha.LostThreshold + TimeSpan.FromSeconds(1);

            var first = ha.Process(message1);
            var second = ha.Process(message2);

            Assert.That(first, Is.Not.Null);
            Assert.That(first!.Count, Is.EqualTo(1));
            Assert.That(second, Is.Not.Null);
            Assert.That(second!.Count, Is.EqualTo(1));
            Assert.That(ha.ErrorCount, Is.EqualTo(1));
            Assert.That(ha.WarningCount, Is.EqualTo(0));
        }
    }
}