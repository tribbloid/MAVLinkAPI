using NUnit.Framework;
using MAVLinkAPI.Routing;

namespace MAVLinkAPI.Routing.Tests
{
    public class IOStreamTests
    {
        [Test]
        public void TestArgsT_Parse()
        {
            // Arrange
            var originalArgs = new IOStream.ArgsT
            {
                protocol = Protocol.Udp,
                address = "localhost:14550"
            };
            var text = originalArgs.URIString;

            // Act
            var parsedArgs = IOStream.ArgsT.Parse(text);

            // Assert
            Assert.AreEqual(originalArgs.protocol, parsedArgs.protocol);
            Assert.AreEqual(originalArgs.address, parsedArgs.address);
        }

        [Test]
        public void TestUDPLocalDefault_Parse()
        {
            // Arrange
            var originalArgs = IOStream.ArgsT.UDPLocalDefault;
            var text = originalArgs.URIString;

            // Act
            var parsedArgs = IOStream.ArgsT.Parse(text);

            // Assert
            Assert.AreEqual(originalArgs.protocol, parsedArgs.protocol);
            Assert.AreEqual(originalArgs.address, parsedArgs.address);
        }
    }
}