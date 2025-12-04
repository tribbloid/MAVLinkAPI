#nullable enable
using System;
using MAVLinkAPI.Util.NullSafety;

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

        public TxMessage<T> MkTxMessage<T>(T data)
        {
            return new TxMessage<T>(data, this);
        }
    }


    public record Signature(
        byte[] Blob, // referring to sig in MAVLinkMessage
        ulong Timestamp // not a DateTime, only useful in warding off replay attacks
    )
    {
    }

    // mavlink msg id is automatically inferred by reflection
    public interface IMessage<out T>
    {
        T Data { get; }
        Component Sender { get; }

        MAVLink.message_info Info { get; }

        public MAVLink.MAVLINK_MSG_ID TypeID => (MAVLink.MAVLINK_MSG_ID)Info.msgid;
    }


    public record RxMessage<T>(
        MAVLink.MAVLinkMessage Raw
    ) : IMessage<T>
    {
        public DateTime RxTime { get; } = Raw?.rxtime ?? DateTime.MinValue;

        private Maybe<T> _data;
        public T Data => _data.Lazy(() => Raw.ToStructure<T>());

        private Maybe<Component> _sender;
        public Component Sender => _sender.Lazy(() => new Component(Raw.sysid, Raw.compid));

        private Maybe<Signature> _signature;

        public Signature Signature =>
            _signature.Lazy(() => new Signature(Raw.sig ?? Array.Empty<byte>(), Raw.sigTimestamp));

        private Maybe<MAVLink.message_info> _info;
        public MAVLink.message_info Info => _info.Lazy(() => IDLookup.Global.ByID[Raw.msgid]);
    }

    public record TxMessage<T>(
        T Data,
        Component Sender
    ) : IMessage<T>
    {
        public MAVLink.message_info Info
        {
            get
            {
                var id1 = IDLookup.Global.ByType[typeof(T)];

                return id1; // TODO: add verified info that also run the lookup by 
            }
        }
    }
}