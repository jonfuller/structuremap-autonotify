using Castle.Core.Interceptor;

namespace StructureMap.AutoNotify.Interception
{
    class PropertySetterInterception : IInterception
    {
        readonly PropertyChangedDecorator _propertyChangedDecorator;
        readonly IInvocation _invocation;

        public PropertySetterInterception(PropertyChangedDecorator propertyChangedDecorator, IInvocation invocation)
        {
            _propertyChangedDecorator = propertyChangedDecorator;
            _invocation = invocation;
        }

        public void Call()
        {
            _invocation.Proceed();
            _propertyChangedDecorator.Notify(_invocation);
            _propertyChangedDecorator.SetDependents(_invocation);
        }
    }
}