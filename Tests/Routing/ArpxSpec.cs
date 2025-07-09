using NUnit.Framework;
using MAVLinkAPI.Routing.Relay;
using System.Collections.Generic;

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

            protected void AssertReserialized(string original, string reserialized)
            {
                Assert.AreEqual(original.Trim(), reserialized.Trim());
            }

            [Test]
            public void SerialRoundtrip_Minimal()
            {
                var profile = Deserialize(MinimalRepresentation);

                var newSerialized = Serialize(profile, true);

                AssertReserialized(MinimalRepresentation, newSerialized);
            }


            [Test]
            public void SerialRoundtrip_Tutorial()
            {
                var profile = Deserialize(TutorialRepresentation);

                var newSerialized = Serialize(profile, true);

                AssertReserialized(TutorialRepresentation, newSerialized);
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
                AssertReserialized(serialized, serialized2);
            }
        }

        [TestFixture]
        public class ArpxYamlSpec : Base
        {
            protected override string MinimalRepresentation => "jobs: {}\nprocesses: {}\nlog_monitors: {}";

            protected override string TutorialRepresentation =>
                @"jobs:
  foo:
    command: >
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
    test: echo ""$ARPX_BUFFER"" | grep -q ""bar""
    ontrigger: quux
";

            protected override string Serialize(Arpx.Profile profile, bool pretty)
            {
                return Arpx.Profile.ToYaml(profile, pretty);
            }

            protected override Arpx.Profile Deserialize(string data)
            {
                return Arpx.Profile.FromYaml(data);
            }
        }

        [TestFixture]
        public class ArpxJsonSpec : Base
        {
            protected override string MinimalRepresentation => "{}";

            protected override string TutorialRepresentation =>
                "{\n" +
                "  \"jobs\": {\n" +
                "    \"foo\": {\n" +
                "      \"command\": \"bar ? baz : qux;\\n[\\n  bar;\\n  baz;\\n  qux;\\n]\\nbar; @quux\\n\"\n" +
                "    }\n" +
                "  },\n" +
                "  \"processes\": {\n" +
                "    \"bar\": {\n" +
                "      \"command\": \"echo bar\"\n" +
                "    },\n" +
                "    \"baz\": {\n" +
                "      \"command\": \"echo baz\"\n" +
                "    },\n" +
                "    \"qux\": {\n" +
                "      \"command\": \"echo qux\"\n" +
                "    },\n" +
                "    \"quux\": {\n" +
                "      \"command\": \"echo quux\"\n" +
                "    }\n" +
                "  },\n" +
                "  \"logMonitors\": {\n" +
                "    \"quux\": {\n" +
                "      \"buffer_size\": 1,\n" +
                "      \"test\": \"echo \\\"$ARPX_BUFFER\\\" | grep -q \\\"bar\\\"\",\n" +
                "      \"ontrigger\": \"quux\"\n" +
                "    }\n" +
                "  }\n" +
                "}";

            protected override string Serialize(Arpx.Profile profile, bool pretty)
            {
                return Arpx.Profile.ToJson(profile, pretty);
            }

            protected override Arpx.Profile Deserialize(string data)
            {
                return Arpx.Profile.FromJson(data);
            }
        }
    }
}