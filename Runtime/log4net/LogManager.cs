using System;

namespace MAVLinkAPI.log4net
{
    internal class LogManager
    {
        internal static ILog GetLogger(Type declaringType)
        {
            return new Log();
        }
    }
}