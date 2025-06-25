#nullable enable
using System.Collections.Generic;
using System.Linq;
using MAVLinkAPI.Util;

namespace MAVLinkAPI.Ext
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T Current, Box<T>? Next)> ZipWithNext<T>(
            this IEnumerable<T> source
        )
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                yield break;

            var current = enumerator.Current;
            while (enumerator.MoveNext())
            {
                yield return (current, enumerator.Current);
                current = enumerator.Current;
            }

            yield return (current, null);
        }

        public static IEnumerable<T>? UnionNullSafe<T>(this IEnumerable<T>? first, IEnumerable<T>? second)
        {
            if (first == null) return second;
            if (second == null) return first;
            return first.Union(second);
        }
    }
}