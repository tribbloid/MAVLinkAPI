using System;
using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {
    }
}

namespace MAVLinkAPI.Routing
{
    public enum SitlArch
    {
        X64Windows,
        X64Linux,
        AppleSilicon
    }

    public record ArduPilotSitlRepo(
        SitlArch arch = SitlArch.X64Linux,
        string Frame = "Plane",
        string Version = "stable-4.5.7"
    )
    {
        public string Url
        {
            get
            {
                string result;

                switch (arch)
                {
                    // case Arch.SITLX64Windows:
                    //     return $"https://firmware.ardupilot.org/{Frame}/{Version}/windows/{Env}/";
                    case SitlArch.X64Linux:
                        result = $"https://firmware.ardupilot.org/{Frame}/{Version}/SITL_x86_64_linux_gnu/ardupilot";
                        break;
                    // case Arch.SITLAppleSilicon:
                    //     return $"https://firmware.ardupilot.org/{Frame}/{Version}/macos/{Env}/";
                    default:
                        throw new NotImplementedException($"architecture {arch} not supported");
                }

                return result;
            }
        }
    }
}