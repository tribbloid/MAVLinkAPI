using System;

namespace MAVLinkAPI.Scripts.log4net
{
    internal class LogManager
    {
        internal static ILog GetLogger(Type declaringType)
        {
            return new Log();
        }
    }
}