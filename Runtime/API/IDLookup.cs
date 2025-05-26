#nullable enable
using System;
using System.Collections.Generic;

namespace MAVLinkAPI.API
{
    public class IDLookup
    {
        public readonly Dictionary<uint, MAVLink.message_info> ByID = new();
        public readonly Dictionary<Type, MAVLink.message_info> ByType = new();

        public static readonly IDLookup Global = new();

        // constructor
        private IDLookup()
        {
            Compile();
        }

        public void Compile()
        {
            var report = new List<string>();
            foreach (var info in MAVLink.MAVLINK_MESSAGE_INFOS)
            {
                ByID.Add(info.msgid, info);
                ByType.Add(info.type, info);
                report.Add($"{info.msgid} -> {info.type.Name}");
            }

            Console.WriteLine("MAVLink message lookup compiled:\n" + string.Join("\n", report));
        }
    }
}