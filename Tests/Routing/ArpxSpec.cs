using NUnit.Framework;
using MAVLinkAPI.Routing.Relay;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using UnityEngine;

namespace MAVLinkAPI.Tests.Routing
{
    public static class ArpxSpec
    {
        public abstract class Base
        {
            protected abstract string MinimalRepresentation { get; }

            protected abstract string TutorialRepresentation { get; }

            protected abstract string Serialize(Arpx.Profile profile, bool pretty);

            protected abstract Arpx.Profile Deserialize(string data);

            // protected abstract void AssertDeserializedMinimalProfile(Arpx.Profile profile);
            //
            // protected abstract void AssertDeserializedEmptyProfile(Arpx.Profile profile);

            protected void AssertReserializedMinimalProfile(string original, string reserialized)
            {
                Assert.AreEqual(original.Trim(), reserialized.Trim());
            }

            [Test]
            public void SerialRoundtrip_Minimal()
            {
                var profile = Deserialize(MinimalRepresentation);

                // AssertDeserializedMinimalProfile(profile);

                var newSerialized = Serialize(profile, true);

                AssertReserializedMinimalProfile(MinimalRepresentation, newSerialized);
            }


            [Test]
            public void SerialRoundtrip_Tutorial()
            {
                var profile = Deserialize(TutorialRepresentation);

                // AssertDeserializedMinimalProfile(profile);

                var newSerialized = Serialize(profile, true);

                AssertReserializedMinimalProfile(TutorialRepresentation, newSerialized);
            }

            [Test]
            public void ObjectRoundtrip_Minimal()
            {
                var emptyProfile = new Arpx.Profile
                {
                    Jobs = new Dictionary<string, Arpx.Job>(),
                    Processes = new Dictionary<string, Arpx.Process>(),
                    LogMonitors = new Dictionary<string, Arpx.LogMonitor>()
                };

                var serialized = Serialize(emptyProfile, false);
                var newProfile = Deserialize(serialized);

                Assert.IsNotNull(newProfile);

                var serialized2 = Serialize(newProfile, false);
                AssertReserializedMinimalProfile(serialized, serialized2);
            }
        }

        [TestFixture]
        public class ArpxYamlSpec : Base
        {
            private readonly ISerializer _serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            private readonly IDeserializer _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            protected override string MinimalRepresentation => "jobs: {}\nprocesses: {}\nlogMonitors: {}";

            protected override string TutorialRepresentation =>
                @"""
jobs:
  foo: |
    bar ? baz : qux;
    [
      bar;
      baz;
      qux;
    ]
    bar; @quux

processes:
  bar:
    command: echo bar
  baz:
    command: echo baz
  qux:
    command: echo qux
  quux:
    command: echo quux

log_monitors:
  quux:
    buffer_size: 1
    test: 'echo ""$ARPX_BUFFER"" | grep -q ""bar""' # or equivalent for your system
    ontrigger: quux

""";

            protected override string Serialize(Arpx.Profile profile, bool pretty)
            {
                return _serializer.Serialize(profile);
            }

            protected override Arpx.Profile Deserialize(string data)
            {
                return _deserializer.Deserialize<Arpx.Profile>(data);
            }

            // protected override void AssertDeserializedMinimalProfile(Arpx.Profile profile)
            // {
            //     Assert.IsNotNull(profile);
            //     Assert.IsNotNull(profile.Jobs);
            //     Assert.IsEmpty(profile.Jobs);
            //     Assert.IsNotNull(profile.Processes);
            //     Assert.IsEmpty(profile.Processes);
            //     Assert.IsNotNull(profile.LogMonitors);
            //     Assert.IsEmpty(profile.LogMonitors);
            // }
            //
            // protected override void AssertDeserializedEmptyProfile(Arpx.Profile profile)
            // {
            //     Assert.IsNotNull(profile.Jobs);
            //     Assert.IsEmpty(profile.Jobs);
            //     Assert.IsNotNull(profile.Processes);
            //     Assert.IsEmpty(profile.Processes);
            //     Assert.IsNotNull(profile.LogMonitors);
            //     Assert.IsEmpty(profile.LogMonitors);
            // }
        }

        [TestFixture]
        public class ArpxJsonSpec : Base
        {
            protected override string MinimalRepresentation => "{}";

            protected override string TutorialRepresentation =>
                @"""
{
  ""jobs"": {
    ""foo"": ""bar ? baz : qux;\n[\n  bar;\n  baz;\n  qux;\n]\nbar; @quux\n""
  },
  ""processes"": {
    ""bar"": {
      ""command"": ""echo bar""
    },
    ""baz"": {
      ""command"": ""echo baz""
    },
    ""qux"": {
      ""command"": ""echo qux""
    },
    ""quux"": {
      ""command"": ""echo quux""
    }
  },
  ""log_monitors"": {
    ""quux"": {
      ""buffer_size"": 1,
      ""test"": ""echo \""$ARPX_BUFFER\"" | grep -q \""bar\"""",
      ""ontrigger"": ""quux""
    }
  }
}
                """;

            protected override string Serialize(Arpx.Profile profile, bool pretty)
            {
                return JsonUtility.ToJson(profile, pretty);
            }

            protected override Arpx.Profile Deserialize(string data)
            {
                return JsonUtility.FromJson<Arpx.Profile>(data);
            }

            // protected override void AssertDeserializedMinimalProfile(Arpx.Profile profile)
            // {
            //     Assert.IsNotNull(profile);
            //     Assert.IsNull(profile.Jobs);
            //     Assert.IsNull(profile.Processes);
            //     Assert.IsNull(profile.LogMonitors);
            // }
            //
            // protected override void AssertDeserializedEmptyProfile(Arpx.Profile profile)
            // {
            //     Assert.IsNull(profile.Jobs);
            //     Assert.IsNull(profile.Processes);
            //     Assert.IsNull(profile.LogMonitors);
            // }
        }
    }
}