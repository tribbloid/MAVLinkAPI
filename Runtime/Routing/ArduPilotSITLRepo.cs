using System;
using System.ComponentModel;

namespace MAVLinkAPI.Routing
{
    public enum SitlArch
    {
        X64Windows,
        X64Linux,
        AppleSilicon
    }

    public record ArduPilotSitlRepo(
        SitlArch Arch = SitlArch.X64Linux,
        string Frame = "Plane",
        string Version = "stable"
    )
    {
        public string Url
        {
            get
            {
                string result;

                switch (Arch)
                {
                    // case Arch.SITLX64Windows:
                    //     return $"https://firmware.ardupilot.org/{Frame}/{Version}/windows/{Env}/";
                    case SitlArch.X64Linux:
                        result = $"https://firmware.ardupilot.org/{Frame}/{Version}/SITL_x86_64_linux_gnu/ardupilot";
                        break;
                    // case Arch.SITLAppleSilicon:
                    //     return $"https://firmware.ardupilot.org/{Frame}/{Version}/macos/{Env}/";
                    default:
                        throw new NotImplementedException($"architecture {Arch} not supported");
                }

                return result;
            }
        }
    }
}