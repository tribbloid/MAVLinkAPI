#nullable enable
using MAVLinkAPI.Util.Resource;

namespace MAVLinkAPI.Tests.Util.Resource
{
    public class CleanableExample : Cleanable
    {
        public static volatile int Counter;

        // default constructor with lifetime argument
        public CleanableExample(
            Lifetime? lifetime = null
        ) : base(lifetime)
        {
        }

        public override void DoClean()
        {
            Counter += 1;
        }
    }
}