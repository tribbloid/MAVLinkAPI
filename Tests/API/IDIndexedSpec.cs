using System.Collections.Generic;
using MAVLinkAPI.API;
using MAVLinkAPI.Util.Text;
using NUnit.Framework;

namespace MAVLinkAPI.Tests.API
{
    public class IDIndexedSpec
    {
        [Test]
        public void ToString_ReturnsCorrectFormat()
        {
            // Arrange
            var index = new IDIndexed<int>(new Dictionary<uint, int>
            {
                { 0, 10 }, // Corresponds to MAVLink.mavlink_heartbeat_t
                { 1, 20 } // Corresponds to MAVLink.mavlink_sys_status_t
            });

            // Act
            var result = index.ToString();

            // Assert
            var expected = "mavlink_heartbeat_t: 10\n" +
                           "mavlink_sys_status_t: 20\n";
            Assert.AreEqual(expected.normaliseLineBreak(), result);
        }
    }
}