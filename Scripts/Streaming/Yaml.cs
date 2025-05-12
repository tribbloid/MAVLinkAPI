using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MAVLinkAPI.Scripts.Streaming
{
    public class Yaml
    {
        public static readonly INamingConvention convention = CamelCaseNamingConvention.Instance;

        public readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(convention)
            .Build();

        public readonly ISerializer Serializer = new SerializerBuilder()
            .WithNamingConvention(convention)
            .Build();

        public string Fwd<T>(T value)
        {
            return Serializer.Serialize(value);
        }

        public T Rev<T>(string yaml)
        {
            var result = Deserializer.Deserialize<T>(yaml);
            return result;
        }
    }
}