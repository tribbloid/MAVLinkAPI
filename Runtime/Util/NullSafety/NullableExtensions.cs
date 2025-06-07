#nullable enable
using System.Collections.Generic;

namespace MAVLinkAPI.Util.NullSafety
{
    public static class NullableExtensions
    {
        // TODO: how to remove this duplicate?
        public static IEnumerable<T> Wrap<T>(this T? nullable) where T : struct
        {
            if (nullable.HasValue)
            {
                var element = nullable.Value;
                yield return element;
            }
        }


        public static IEnumerable<T> Wrap<T>(this T? nullable) where T : class
        {
            if (nullable != null) yield return nullable;
        }
    }

    // public static class Implicits
    // TODO: advanced C# feature, may be enabled later
    //  see https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/user-defined-conversion-operators
}