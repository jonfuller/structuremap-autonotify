using Castle.Core.Interceptor;

namespace StructureMap.AutoNotify.Interception
{
    class PropertySetterInterception : IInterception
    {
        readonly PropertyChangedInterceptor _propertyChangedInterceptor;
        readonly IInvocation _invocation;

        public PropertySetterInterception(PropertyChangedInterceptor propertyChangedInterceptor, IInvocation invocation)
        {
            _propertyChangedInterceptor = propertyChangedInterceptor;
            _invocation = invocation;
        }

        public void Call()
        {
            _invocation.Proceed();
            _propertyChangedInterceptor.Notify(_invocation);
            _propertyChangedInterceptor.SetDependents(_invocation);
        }
    }
}