using Castle.Core.Interceptor;
using log4net;

namespace StructureMap.AutoNotify.Interception
{
    class OnlyOnChangePropertySetterInterception : IInterception
    {
        readonly PropertyChangedInterceptor _propertyChangedInterceptor;
        readonly IInvocation _invocation;
        readonly string _propertyName;
        readonly ILog _logger;

        public OnlyOnChangePropertySetterInterception(PropertyChangedInterceptor propertyChangedInterceptor, IInvocation invocation, string propertyName, ILog logger)
        {
            _propertyChangedInterceptor = propertyChangedInterceptor;
            _invocation = invocation;
            _propertyName = propertyName;
            _logger = logger;
        }

        public void Call()
        {
            object oldValue = _invocation.InvocationTarget
                .GetType()
                .GetProperty(_propertyName)
                .GetValue(_invocation.InvocationTarget, new object[0]);

            _invocation.Proceed();
            var newValue = _invocation.GetArgumentValue(0);

            _logger.DebugFormat("Old value: {0}", oldValue);
            _logger.DebugFormat("New value: {0}", newValue);

            if(AreEqual(oldValue, newValue))
            {
                _logger.DebugFormat("Values are 'equal', not firing PropertyChanged");
                return;
            }

            _logger.DebugFormat("Values are not equal.");
            _propertyChangedInterceptor.Notify(_invocation);
            _propertyChangedInterceptor.SetDependents(_invocation);
        }

        private static bool AreEqual(object oldValue, object newValue)
        {
            return (oldValue == null && newValue == null)
                   || (oldValue != null && oldValue.Equals(newValue))
                   || (newValue != null && newValue.Equals(oldValue));
        }
    }
}