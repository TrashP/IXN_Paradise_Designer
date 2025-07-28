using System.Collections.Generic;
using System.Diagnostics;

namespace Polyperfect.Common
{
    public static class CollectionExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> that, T val)
        {
            var i = 0;
            foreach (var item in that)
            {
                if (item.Equals(val))
                    return i;
                i++;
            }

            return -1;
        }

        public static VALUE GetDataOrDefault<KEY, VALUE>(this IReadOnlyDictionary<KEY, VALUE> that, KEY key)
        {
            if (key == null) return default;

            that.TryGetValue(key, out var ret);
            return ret;
        }

        public static VALUE GetDataOrDefault<KEY, VALUE>(this IDictionary<KEY, VALUE> that, KEY key)
        {
            if (key == null) return default;

            that.TryGetValue(key, out var ret);
            return ret;
        }
        

        public static IEnumerable<VALUE> Empty<VALUE>()
        {
            yield break;
        }

        public static void RemoveAtSwapback<T>(this IList<T> that, int index)
        {
            var endIndex = that.Count - 1;
            that[index] = that[endIndex];
            that.RemoveAt(endIndex);
        }
    }
}