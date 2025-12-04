using System.Collections.Generic;
using System.Linq;
using MAVLinkAPI.API;
using MAVLinkAPI.Routing;
using NUnit.Framework;

namespace MAVLinkAPI.Tests.API
{
    public class ReaderSpec
    {


        [Test]
        public void RawReadSource_EmitsCorrectMessage()
        {
            // Arrange
            var message = MAVFunction.MockHeartbeat();
            var uplink = new Uplink.Dummy(new List<MAVLink.MAVLinkMessage> { message });

            // Act
            var result = uplink.RawReadSource.ToList();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(message.msgid, result[0].msgid);
        }


        [Test]
        public void DirectOutput()
        {
            // Arrange
            var message = MAVFunction.MockHeartbeat();
            var uplink = new Uplink.Dummy(new List<MAVLink.MAVLinkMessage> { message });
            var mavFunction = MAVFunction.On<MAVLink.mavlink_heartbeat_t>();
            var reader = uplink.Read(mavFunction);

            var result = reader.Drain();

            // Assert
            Assert.AreEqual(1, result.Count);
            // Assert.AreEqual("Transformed", result[0]);
        }

        [Test]
        public void Select_TransformsOutput()
        {
            // Arrange
            var message = MAVFunction.MockHeartbeat();
            var uplink = new Uplink.Dummy(new List<MAVLink.MAVLinkMessage> { message });
            var mavFunction = MAVFunction.On<MAVLink.mavlink_heartbeat_t>().Select((m, p) => 1);
            var reader = uplink.Read(mavFunction);

            // Act
            var stringReader = reader.Select((m, i) => "Transformed");
            var result = stringReader.Drain();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Transformed", result[0]);
        }

        [Test]
        public void SelectMany_TransformsAndFlattensOutput()
        {
            // Arrange
            var message = MAVFunction.MockHeartbeat();
            var uplink = new Uplink.Dummy(new List<MAVLink.MAVLinkMessage> { message });
            var mavFunction = MAVFunction.On<MAVLink.mavlink_heartbeat_t>().Select((m, p) => 1);
            var reader = uplink.Read(mavFunction);

            // Act
            var listReader = reader.SelectMany((m, i) => new List<string> { "A", "B" });
            var result = listReader.Drain();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains("A"));
            Assert.IsTrue(result.Contains("B"));
        }

        [Test]
        public void Union_WithDifferentUplinks_CombinesSources()
        {
            // Arrange
            var uplink1 = new Uplink.Dummy();
            var uplink2 = new Uplink.Dummy();
            var func = MAVFunction.On<MAVLink.mavlink_heartbeat_t>().Select((m, p) => 1);
            var reader1 = new Reader<int>(uplink1, func);
            var reader2 = new Reader<int>(uplink2, func);

            // Act
            var unionReader = reader1.Union(reader2);

            // Assert
            Assert.AreEqual(2, unionReader.Sources.Count);
            Assert.IsTrue(unionReader.Sources.ContainsKey(uplink1));
            Assert.IsTrue(unionReader.Sources.ContainsKey(uplink2));
        }

        [Test]
        public void OrElse_WithDifferentUplinks_CombinesSources()
        {
            // Arrange
            var uplink1 = new Uplink.Dummy();
            var uplink2 = new Uplink.Dummy();
            var func = MAVFunction.On<MAVLink.mavlink_heartbeat_t>().Select((m, p) => 1);
            var reader1 = new Reader<int>(uplink1, func);
            var reader2 = new Reader<int>(uplink2, func);

            // Act
            var orElseReader = reader1.OrElse(reader2);

            // Assert
            Assert.AreEqual(2, orElseReader.Sources.Count);
            Assert.IsTrue(orElseReader.Sources.ContainsKey(uplink1));
            Assert.IsTrue(orElseReader.Sources.ContainsKey(uplink2));
        }

        [Test]
        public void Union_WithSameUplink_CombinesFunctions()
        {
            // Arrange
            var uplink = new Uplink.Dummy();
            var func1 = MAVFunction.On<MAVLink.mavlink_heartbeat_t>().Select((m, p) => 1);
            var func2 = MAVFunction.On<MAVLink.mavlink_system_time_t>().Select((m, p) => 2);
            var reader1 = new Reader<int>(uplink, func1);
            var reader2 = new Reader<int>(uplink, func2);

            // Act
            var unionReader = reader1.Union(reader2);

            // Assert
            Assert.AreEqual(1, unionReader.Sources.Count);
        }

        [Test]
        public void OrElse_WithSameUplink_CombinesFunctions()
        {
            // Arrange
            var uplink = new Uplink.Dummy();
            var func1 = MAVFunction.On<MAVLink.mavlink_heartbeat_t>().Select((m, p) => 1);
            var func2 = MAVFunction.On<MAVLink.mavlink_system_time_t>().Select((m, p) => 2);
            var reader1 = new Reader<int>(uplink, func1);
            var reader2 = new Reader<int>(uplink, func2);

            // Act
            var orElseReader = reader1.OrElse(reader2);

            // Assert
            Assert.AreEqual(1, orElseReader.Sources.Count);
        }
    }
}