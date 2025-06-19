#nullable enable
using System;
using System.Collections.Generic;

namespace MAVLinkAPI.API
{
    public struct Component
    {
        // our target sysid
        public byte SystemID;

        // our target compid
        public byte ComponentID;

        public static Component Gcs(byte compid = 0)
        {
            return new Component
            {
                SystemID = 255,
                ComponentID = compid
            };
        }

        public static Component Gcs0 = Gcs();

        public Message<T> ToMessage<T>(T data) where T : struct
        {
            return new Message<T>
            {
                Data = data,
                Sender = this
            };
        }
    }


    // mavlink msg id is automatically inferred by reflection
    public struct Message<T> where T : struct
    {
        public T Data;
        public Component Sender;
        public DateTime RxTime;

        public MAVLink.message_info Info
        {
            get
            {
                var id1 = IDLookup.Global.ByType[typeof(T)];

                return id1;
                // TODO: add verified info that also run the lookup by 
            }
        }

        public MAVLink.MAVLINK_MSG_ID TypeID => (MAVLink.MAVLINK_MSG_ID)Info.msgid;

        public static Message<T> FromRaw(MAVLink.MAVLinkMessage msg)
        {
            var sender = new Component
            {
                SystemID = msg.sysid,
                ComponentID = msg.compid
            };

            return new Message<T>
            {
                Data = (T)msg.data,
                Sender = sender,
                RxTime = msg.rxtime
            };
        }
    }


    public static class RawMessageExtension
    {
        public static Message<T> OfType<T>(this MAVLink.MAVLinkMessage msg) where T : struct
        {
            return Message<T>.FromRaw(msg);
        }
    }
}