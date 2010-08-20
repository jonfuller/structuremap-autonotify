using System.ComponentModel;
using Castle.Core.Interceptor;

namespace StructureMap.AutoNotify.Interception
{
    class PropertyChangedRemoveInterception : IInterception
    {
        readonly PropertyChangedDecorator _propertyChangedDecorator;
        readonly IInvocation _invocation;

        public PropertyChangedRemoveInterception(PropertyChangedDecorator propertyChangedDecorator, IInvocation invocation)
        {
            _propertyChangedDecorator = propertyChangedDecorator;
            _invocation = invocation;
        }

        public void Call()
        {
            var onPropertyChanged = (PropertyChangedEventHandler)_invocation.GetArgumentValue(0);
            _propertyChangedDecorator.PropertyChanged -= onPropertyChanged;
        }
    }
}