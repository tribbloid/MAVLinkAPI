#nullable enable
using System.Collections.Generic;
using MAVLinkAPI.API;

namespace MAVLinkAPI.API.Fn
{
    public class RawT : MAVFunction<MAVLink.MAVLinkMessage>
    {
        protected override IDIndexed<CaseFn> MkTopics()
        {
            return new IDIndexed<CaseFn>();
        }

        protected override CaseFn OtherCase => m => new List<MAVLink.MAVLinkMessage> { m };
    }
}
