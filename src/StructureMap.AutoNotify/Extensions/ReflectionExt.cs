using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace StructureMap.AutoNotify.Extensions
{
    public static class ReflectionExt
    {
        public static string Name<TObj, TProp>(this Expression<Func<TObj, TProp>> propExpr)
        {
            var body = (MemberExpression)propExpr.Body;
            return body.Member.Name;
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
    }
}