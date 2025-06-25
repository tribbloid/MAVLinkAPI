#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Newtonsoft.Json.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace MAVLinkAPI.Routing.Relay
{
    public static class Arpx
    {
        [Serializable]
        public class Task
        {
            public List<Process>? Processes { get; init; } = null;
        }

        [Serializable]
        public class Job
        {
            public string? Command { get; init; }

            public List<Task>? Tasks { get; init; }
        }

        [Serializable]
        public class LogMonitor
        {
            public string Name { get; init; } = null!;

            [JsonProperty("buffer_size")]
            [YamlMember(Alias = "buffer_size")]
            public int BufferSize { get; init; }

            public string Test { get; init; } = null!;

            [JsonProperty("ontrigger")]
            [YamlMember(Alias = "ontrigger")]
            public string? OnTrigger { get; init; }
        }

        [Serializable]
        public class Process
        {
            public string Name { get; init; } = null!;

            public string Command { get; init; } = null!;

            public string? Cwd { get; init; }

            [JsonProperty("log_monitors")]
            [YamlMember(Alias = "log_monitors")]
            public List<string>? LogMonitors { get; init; }

            [JsonProperty("on_succeed")]
            [YamlMember(Alias = "on_succeed")]
            public string? OnSucceed { get; init; }

            [JsonProperty("on_fail")]
            [YamlMember(Alias = "on_fail")]
            public string? OnFail { get; init; }
        }

        [Serializable]
        public class Profile
        {
            public Dictionary<string, Job> Jobs { get; init; } = null!;

            public Dictionary<string, Process> Processes { get; init; } = null!;

            public Dictionary<string, LogMonitor>? LogMonitors { get; init; }


            private class OmitNullPropertiesTypeInspector : TypeInspectorSkeleton
            {
                private readonly ITypeInspector _innerTypeDescriptor;

                public OmitNullPropertiesTypeInspector(ITypeInspector innerTypeDescriptor)
                {
                    _innerTypeDescriptor = innerTypeDescriptor;
                }

                public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
                {
                    var properties = _innerTypeDescriptor.GetProperties(type, container);
                    if (container == null) return properties;

                    return properties.Where(p =>
                    {
                        var valueDescriptor = (IObjectDescriptor)p.Read(container);

                        if (valueDescriptor == null || valueDescriptor.Value == null) return false;

                        if (valueDescriptor.Value is string stringValue) return !string.IsNullOrWhiteSpace(stringValue);

                        return true;
                    });
                }
            }

            // YAML serialization
            public static string ToYaml(Profile profile, bool pretty = true)
            {
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .WithTypeInspector(inner => new OmitNullPropertiesTypeInspector(inner))
                    // .WithDefaultScalarStyle(ScalarStyle.Plain)
                    .Build();
                return serializer.Serialize(profile);
            }

            public static Profile FromYaml(string data)
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();
                return deserializer.Deserialize<Profile>(data);
            }

            // JSON serialization
            public static string ToJson(Profile profile, bool pretty = true)
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                };
                return JsonConvert.SerializeObject(profile, pretty ? Formatting.Indented : Formatting.None, settings);
            }

            public static Profile FromJson(string data)
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                };
                return JsonConvert.DeserializeObject<Profile>(data, settings)!;
            }
        }
    }
}