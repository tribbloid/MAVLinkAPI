#nullable enable
using System;
using System.Collections.Generic;
using MAVLinkAPI.Util;

namespace MAVLinkAPI.API
{
    public struct IDIndexed<T>
    {
        // TODO: do I need to index by systemID and componentID?
        public readonly IDLookup Lookup;
        public readonly Dictionary<uint, T> Index;

        public IDIndexed( Dictionary<uint, T>? index = null, IDLookup? lookup = null)
        {
            Lookup = lookup ?? IDLookup.Global;
            Index = index ?? new Dictionary<uint, T>();
        }
        
        // default constructor

        public class Accessor : HasOuter<IDIndexed<T>>
        {
            public uint ID;

            public T? Value
            {
                get => Outer.Index[ID];
                set => Outer.Index[ID] = value;
            }

            public T? ValueOrDefault => Outer.Index.GetValueOrDefault(ID);

            public T ValueOr(T fallback)
            {
                return Outer.Index.GetValueOrDefault(ID, fallback);
            }

            public T ValueOrInsert(Func<T?> fallback)
            {
                var index = Outer.Index;
                if (index.TryGetValue(ID, out var existing)) return existing;

                index[ID] = fallback();
                return index[ID];
            }

            public T? ValueOrInsertDefault()
            {
                return ValueOrInsert(() => default);
            }

            public void Remove()
            {
                Outer.Index.Remove(ID);
            }

            public MAVLink.message_info Info => Outer.Lookup.ByID[ID];
        }

        public readonly Accessor Get(uint id)
        {
            return new Accessor { Outer = this, ID = id };
        }

        public readonly Accessor Get<TMav>() where TMav : struct
        {
            var id = IDLookup.Global.ByType[typeof(TMav)].msgid;
            return Get(id);
        }

        // do we need by systemID and componentID?

    }
}