#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using MAVLinkAPI.Util;

namespace MAVLinkAPI.API
{
    public record IDIndexed<T>
    {
        // TODO: do I need to index by systemID and componentID?
        public IDLookup Lookup { get; init; }
        public Dictionary<uint, T> Index { get; init; }

        // public IDIndexed()
        // {
        //     Lookup = IDLookup.Global;
        //     Index = new Dictionary<uint, T>();
        // }

        public IDIndexed(Dictionary<uint, T>? index = null, IDLookup? lookup = null)
        {
            Lookup = lookup ?? IDLookup.Global;
            Index = index ?? new Dictionary<uint, T>();
        }

        public class Accessor : HasOuter<IDIndexed<T>>
        {
            public readonly uint ID;

            public Accessor(IDIndexed<T> outer, uint id)
            {
                Outer = outer;
                ID = id;
            }

            public T? Value
            {
                get => Outer.Index.GetValueOrDefault(ID);
                set
                {
                    if (value is not null)
                        Outer.Index[ID] = value;
                    else
                        Outer.Index.Remove(ID);
                }
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

                index[ID] = fallback()!;
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

        public Accessor Get(uint id)
        {
            return new Accessor(this, id);
        }

        public Accessor Get<TMav>() where TMav : struct
        {
            var id = IDLookup.Global.ByType[typeof(TMav)].msgid;
            return Get(id);
        }

        public Dictionary<Type, T> TypeToValue()
        {
            var result = new Dictionary<Type, T>();
            foreach (var (id, value) in Index)
            {
                if (!Lookup.ByID.TryGetValue(id, out var info)) continue;

                result[info.type] = value;
            }

            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var (type, value) in TypeToValue()) sb.AppendLine($"{type.Name}: {value}");

            return sb.ToString();
        }

        // do we need filtering by systemID and componentID?
    }
}