using System;

namespace StructureMap.AutoNotify.Extensions
{
    public static class Extensions
    {
        public static T Tap<T>(this T target, Action<T> tap)
        {
            tap(target);
            return target;
        }

        public static object GetInstance(this IContext session, Type instanceType)
        {
            var openMethod = typeof(IContext).GetMethod("GetInstance", new Type[0]);
            var closedMethod = openMethod.MakeGenericMethod(instanceType);

            return closedMethod.Invoke(session, new object[0]);
        }
    }
}