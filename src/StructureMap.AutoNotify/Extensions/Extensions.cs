using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace StructureMap.AutoNotify.Extensions
{
    public static class Extensions
    {
        public static T Tap<T>(this T target, Action<T> tap)
        {
            tap(target);
            return target;
        }

        public static void Each<T>(this IEnumerable<T> target, Action<T> action)
        {
            foreach(var item in target)
                action(item);
        }

        public static ConstructorInfo GetGreediestCtor(this Type target)
        {
            return target.GetConstructors().WithMax(ctor => ctor.GetParameters().Length);
        }

        public static bool HasAttribute<TAttr>(this Type type)
        {
            return type.GetCustomAttributes(typeof(TAttr), true).Length > 0;
        }

        public static TAttr GetAttribute<TAttr>(this Type type)
        {
            return (TAttr)type.GetCustomAttributes(typeof(TAttr), true).FirstOrDefault();
        }

        public static bool HasAttribute<TAttr>(this PropertyInfo property)
        {
            return property.GetCustomAttributes(typeof(TAttr), true).Length > 0;
        }

        public static TAttr GetAttribute<TAttr>(this PropertyInfo property)
        {
            return (TAttr)property.GetCustomAttributes(typeof(TAttr), true).FirstOrDefault();
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

        public static bool IsEmpty(this string target)
        {
            return target.Length == 0;
        }

        public static bool IsNotEmpty(this string target)
        {
            return target.Length > 0;
        }

        public static string Name<TObj, TProp>(this Expression<Func<TObj, TProp>> propExpr)
        {
            var body = (MemberExpression)propExpr.Body;
            return body.Member.Name;
        }
    }
}