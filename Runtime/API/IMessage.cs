#nullable enable
using System;

namespace MAVLinkAPI.API
{
    public record Component(
        // our target sysid
        byte SystemID,
        // our target compid
        byte ComponentID)
    {
        public static Component Gcs(byte compid = 0)
        {
            return new Component(255, compid);
        }

        public static Component Gcs0 = Gcs();

        public Message<T> ToMessage<T>(T data) where T : struct
        {
            return new Message<T>(data, this);
        }
    }


    // mavlink msg id is automatically inferred by reflection
    public interface IMessage<out T>
    {
        T Data { get; }
        Component Sender { get; }

        DateTime RxTime { get; }

        MAVLink.message_info Info { get; }

        public MAVLink.MAVLINK_MSG_ID TypeID => (MAVLink.MAVLINK_MSG_ID)Info.msgid;
    }

    public record Message<T>(
        T Data,
        Component Sender,
        DateTime? RxTimeOrNull = null
    ) : IMessage<T> where T : struct
    {
        public DateTime RxTime => RxTimeOrNull ?? DateTime.UtcNow;

        public MAVLink.message_info Info
        {
            get
            {
                var id1 = IDLookup.Global.ByType[typeof(T)];

                return id1; // TODO: add verified info that also run the lookup by 
            }
        }


        public static Message<T> FromRaw(MAVLink.MAVLinkMessage msg)
        {
            var sender = new Component(msg.sysid, msg.compid);

            return new Message<T>((T)msg.data, sender, msg.rxtime);
        }
    }
}