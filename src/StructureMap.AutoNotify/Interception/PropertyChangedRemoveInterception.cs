using System.ComponentModel;
using Castle.Core.Interceptor;

namespace StructureMap.AutoNotify.Interception
{
    class PropertyChangedRemoveInterception : IInterception
    {
        readonly PropertyChangedInterceptor _propertyChangedInterceptor;
        readonly IInvocation _invocation;

        public PropertyChangedRemoveInterception(PropertyChangedInterceptor propertyChangedInterceptor, IInvocation invocation)
        {
            _propertyChangedInterceptor = propertyChangedInterceptor;
            _invocation = invocation;
        }

        public void Call()
        {
            var onPropertyChanged = (PropertyChangedEventHandler)_invocation.GetArgumentValue(0);
            _propertyChangedInterceptor.PropertyChanged -= onPropertyChanged;
        }
    }
}