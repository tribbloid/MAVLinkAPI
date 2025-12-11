#nullable enable
using System.Collections.Generic;
using System.Linq;
using MAVLinkAPI.API;

namespace MAVLinkAPI.API.Fn
{
    public class OnT<T> : Pipe<RxMessage<T>> where T : struct
    {
        public readonly Pipe<MAVLink.MAVLinkMessage> Prev;

        public OnT(Pipe<MAVLink.MAVLinkMessage> prev)
        {
            Prev = prev;
        }

        protected override IDIndexed<CaseFn> MkTopics()
        {
            var result = new IDIndexed<CaseFn>();
            CaseFn topic = message =>
            {
                var ms = Prev.Process(message);
                var res = ms.Select(m => new RxMessage<T>(m)).ToList();

                return res;
            };
            result.Get<T>().Value = topic;
            return result;
        }
    }
}