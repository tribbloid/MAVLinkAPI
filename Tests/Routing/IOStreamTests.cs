using MAVLinkAPI.Routing;
using NUnit.Framework;

namespace MAVLinkAPI.Tests.Routing
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