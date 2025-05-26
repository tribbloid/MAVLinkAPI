#nullable enable
using System;
using System.Collections.Generic;

namespace MAVLinkAPI.Ext
{
    public class ListOf<T>
    {
        public static List<T> FromTuple<T1, T2>(Tuple<T1, T2> tuple)
        {
            var list = new List<T>();
            if (tuple.Item1 is T item1) list.Add(item1);
            if (tuple.Item2 is T item2) list.Add(item2);
            return list;
        }

        public static List<T> FromTuple<T1, T2, T3>(Tuple<T1, T2, T3> tuple)
        {
            var list = new List<T>();
            if (tuple.Item1 is T item1) list.Add(item1);
            if (tuple.Item2 is T item2) list.Add(item2);
            if (tuple.Item3 is T item3) list.Add(item3);
            return list;
        }

        public static List<T> FromTuple<T1, T2, T3, T4>(Tuple<T1, T2, T3, T4> tuple)
        {
            var list = new List<T>();
            if (tuple.Item1 is T item1) list.Add(item1);
            if (tuple.Item2 is T item2) list.Add(item2);
            if (tuple.Item3 is T item3) list.Add(item3);
            if (tuple.Item4 is T item4) list.Add(item4);
            return list;
        }

        public static List<T> FromTuple<T1, T2, T3, T4, T5>(Tuple<T1, T2, T3, T4, T5> tuple)
        {
            var list = new List<T>();
            if (tuple.Item1 is T item1) list.Add(item1);
            if (tuple.Item2 is T item2) list.Add(item2);
            if (tuple.Item3 is T item3) list.Add(item3);
            if (tuple.Item4 is T item4) list.Add(item4);
            if (tuple.Item5 is T item5) list.Add(item5);
            return list;
        }

        public static List<T> FromTuple<T1, T2, T3, T4, T5, T6>(Tuple<T1, T2, T3, T4, T5, T6> tuple)
        {
            var list = new List<T>();
            if (tuple.Item1 is T item1) list.Add(item1);
            if (tuple.Item2 is T item2) list.Add(item2);
            if (tuple.Item3 is T item3) list.Add(item3);
            if (tuple.Item4 is T item4) list.Add(item4);
            if (tuple.Item5 is T item5) list.Add(item5);
            if (tuple.Item6 is T item6) list.Add(item6);
            return list;
        }

        public static List<T> FromTuple<T1, T2, T3, T4, T5, T6, T7>(Tuple<T1, T2, T3, T4, T5, T6, T7> tuple)
        {
            var list = new List<T>();
            if (tuple.Item1 is T item1) list.Add(item1);
            if (tuple.Item2 is T item2) list.Add(item2);
            if (tuple.Item3 is T item3) list.Add(item3);
            if (tuple.Item4 is T item4) list.Add(item4);
            if (tuple.Item5 is T item5) list.Add(item5);
            if (tuple.Item6 is T item6) list.Add(item6);
            if (tuple.Item7 is T item7) list.Add(item7);
            return list;
        }
    }
}