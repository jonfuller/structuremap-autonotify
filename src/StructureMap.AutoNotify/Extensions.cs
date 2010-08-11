using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StructureMap.AutoNotify
{
    public static class Extensions
    {
        public static ConstructorInfo GetGreediestCtor(this Type target)
        {
            return target.GetConstructors().WithMax(ctor => ctor.GetParameters().Length);
        }

        public static bool HasAttribute<TAttr>(this Type type)
        {
            return type.GetCustomAttributes(typeof(TAttr), true).Length > 0;
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

        public static object GetInstance(this IContext session, Type instanceType)
        {
            var openMethod = typeof(IContext).GetMethod("GetInstance", new Type[0]);
            var closedMethod = openMethod.MakeGenericMethod(instanceType);

            return closedMethod.Invoke(session, new object[0]);
        }

        public static string Join(this IEnumerable<string> target, string separator)
        {
            return string.Join(separator, target.ToArray());
        }
    }
}