using Castle.Core.Interceptor;
using log4net;
using StructureMap.AutoNotify.Extensions;

namespace StructureMap.AutoNotify.Interception
{
    public static class Interception
    {
        public static IInterception Create(PropertyChangedDecorator propertyChangedDecorator, IInvocation invocation, FireOptions fireOption, ILog log)
        {
            if(invocation.IsPropertyChangedAdd())
                return new PropertyChangedAddInterception(propertyChangedDecorator, invocation);
            if(invocation.IsPropertyChangedRemove())
                return new PropertyChangedRemoveInterception(propertyChangedDecorator, invocation);
            if(invocation.IsPropertySetter() && FireOptions.OnlyOnChange == fireOption)
                return new OnlyOnChangePropertySetterInterception(propertyChangedDecorator, invocation, invocation.PropertyName(), log);
            if(invocation.IsPropertySetter())
                return new PropertySetterInterception(propertyChangedDecorator, invocation);
            return new InvocationInterception(invocation);
        }
    }
}