using Castle.Core.Interceptor;

namespace StructureMap.AutoNotify.Extensions
{
    static class InvocationExt
    {
        const string SetPrefix = "set_";

        public static bool IsPropertyChangedAdd(this IInvocation invocation)
        {
            return invocation.Method.Name == "add_PropertyChanged";
        }

        public static bool IsPropertyChangedRemove(this IInvocation invocation)
        {
            return invocation.Method.Name == "remove_PropertyChanged";
        }

        public static bool IsPropertySetter(this IInvocation invocation)
        {
            return invocation.Method.IsSpecialName && invocation.Method.Name.StartsWith(SetPrefix);
        }

        public static string PropertyName(this IInvocation invocation)
        {
            return invocation.Method.Name.Substring(SetPrefix.Length);
        }

        public static object GetCurrentValue(this IInvocation propertySetInvocation)
        {
            return propertySetInvocation.InvocationTarget
                .GetType()
                .GetProperty(propertySetInvocation.PropertyName())
                .GetValue(propertySetInvocation.InvocationTarget, new object[0]);
        }
    }
}