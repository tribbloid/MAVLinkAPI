#nullable enable

using System;
using System.Collections.Generic;

namespace MAVLinkAPI.Routing.Relay
{
    public static class Arpx
    {
        [Serializable]
        public class Task
        {
            public List<Process> Processes { get; init; } = null!;
        }

        [Serializable]
        public class Job
        {
            public List<Task> Tasks { get; init; } = null!;
        }

        [Serializable]
        public class LogMonitor
        {
            public int BufferSize { get; init; }
            public string Name { get; init; } = null!;
            public string OnTrigger { get; init; } = null!;
            public string Test { get; init; } = null!;
        }

        [Serializable]
        public class Process
        {
            public List<string> LogMonitors { get; init; } = null!;
            public string Name { get; init; } = null!;
            public string Command { get; init; } = null!;
            public string Cwd { get; init; } = null!;
            public string OnSucceed { get; init; } = null!;
            public string OnFail { get; init; } = null!;
        }

        [Serializable]
        public class Profile
        {
            public Dictionary<string, Job> Jobs { get; init; } = null!;
            public Dictionary<string, Process> Processes { get; init; } = null!;
            public Dictionary<string, LogMonitor> LogMonitors { get; init; } = null!;
        }
    }
}