using System;
using System.Collections.Generic;
using System.Linq;

namespace StructureMap.AutoNotify.Extensions
{
    public static class EnumerableExt
    {
        public static void Each<T>(this IEnumerable<T> target, Action<T> action)
        {
            foreach(var item in target)
                action(item);
        }

        public static T WithMax<T>(this IEnumerable<T> target, Func<T, int> selector)
        {
            int max = -1;
            T currentMax = default(T);

            foreach(var item in target)
            {
                var current = selector(item);
                if(current <= max)
                    continue;

                max = current;
                currentMax = item;
            }

            return currentMax;
        }

        public static string Join(this IEnumerable<string> target, string separator)
        {
            return String.Join(separator, target.ToArray());
        }

        public static bool IsNotEmpty<T>(this IEnumerable<T> target)
        {
            return target.Count() > 0;
        }
    }
}