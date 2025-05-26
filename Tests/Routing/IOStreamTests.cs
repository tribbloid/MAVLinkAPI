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
            var originalArgs = new IOStream.ArgsT(
                IOStream.Protocol.Udp,
                "localhost:14550"
            );
            var text = originalArgs.URIString;

            // Act
            var parsedArgs = IOStream.ArgsT.Parse(text);

            // Assert
            Assert.AreEqual(originalArgs.Protocol, parsedArgs.Protocol);
            Assert.AreEqual(originalArgs.Address, parsedArgs.Address);
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
            Assert.AreEqual(originalArgs.Protocol, parsedArgs.Protocol);
            Assert.AreEqual(originalArgs.Address, parsedArgs.Address);
        }
    }
}