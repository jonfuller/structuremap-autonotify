using Castle.Core.Interceptor;
using log4net;
using StructureMap.AutoNotify.Extensions;

namespace StructureMap.AutoNotify.Interception
{
    public static class Interception
    {
        public static IInterception Create(PropertyChangedInterceptor propertyChangedInterceptor, IInvocation invocation, FireOptions fireOption, ILog log)
        {
            if(invocation.IsPropertyChangedAdd())
                return new PropertyChangedAddInterception(propertyChangedInterceptor, invocation);
            if(invocation.IsPropertyChangedRemove())
                return new PropertyChangedRemoveInterception(propertyChangedInterceptor, invocation);
            if(invocation.IsPropertySetter() && FireOptions.OnlyOnChange == fireOption)
                return new OnlyOnChangePropertySetterInterception(propertyChangedInterceptor, invocation, log).WrapWith(
                    new PropertyIsINotifyInterception(propertyChangedInterceptor, invocation));
            if(invocation.IsPropertySetter())
                return new PropertySetterInterception(propertyChangedInterceptor, invocation).WrapWith(
                    new PropertyIsINotifyInterception(propertyChangedInterceptor, invocation));
            return new InvocationInterception(invocation);
        }

        public static IInterception WrapWith(this IInterception target, IWrappingInterception wrapper)
        {
            return new WrappingInterception(wrapper, target);
        }
    }
}