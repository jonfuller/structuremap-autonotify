using System.ComponentModel;
using Castle.Core.Interceptor;
using log4net;

namespace StructureMap.AutoNotify
{
    public class PropertyChangedDecorator : IInterceptor
    {
        readonly FireOptions _fireOption;
        static readonly ILog logger = LogManager.GetLogger(typeof(PropertyChangedDecorator));

        public PropertyChangedDecorator(FireOptions fireOption)
        {
            _fireOption = fireOption;
        }

        const string SetPrefix = "set_";

        public void Intercept(IInvocation invocation)
        {
            if(IsPropertyChangedAdd(invocation))
            {
                var onPropertyChanged = (PropertyChangedEventHandler)invocation.GetArgumentValue(0);
                logger.DebugFormat("{0} subscribed to an autonotify", onPropertyChanged.Target.GetType().Name);
                PropertyChanged += onPropertyChanged;
                return;
            }
            if(IsPropertyChangedRemove(invocation))
            {
                var onPropertyChanged = (PropertyChangedEventHandler)invocation.GetArgumentValue(0);
                logger.DebugFormat("{0} unsubscribed to an autonotify", onPropertyChanged.Target.GetType().Name);
                PropertyChanged -= onPropertyChanged;
                return;
            }

            object oldValue = null;

            if(IsPropertySetter(invocation))
            {
                oldValue = invocation.InvocationTarget
                    .GetType()
                    .GetProperty(GetPropertyName(invocation))
                    .GetValue(invocation.InvocationTarget, new object[0]);
            }

            invocation.Proceed();
            if(IsPropertySetter(invocation))
            {
                var propertyName = GetPropertyName(invocation);

                if(FireOptions.Always == _fireOption)
                {
                    logger.DebugFormat("Firing PropertyChanged for {0}.{1}",
                        invocation.InvocationTarget.GetType().Name,
                        propertyName);

                    PropertyChanged(invocation.InvocationTarget, new PropertyChangedEventArgs(propertyName));

                    return;
                }

                if(FireOptions.OnlyOnChange == _fireOption)
                {
                    var newValue = invocation.GetArgumentValue(0);

                    logger.DebugFormat("Old value: {0}", oldValue);
                    logger.DebugFormat("New value: {0}", newValue);

                    if((oldValue == null && newValue == null)
                        || (oldValue != null && oldValue.Equals(newValue))
                        || (newValue != null && newValue.Equals(oldValue)))
                    {
                        logger.DebugFormat("Values are 'equal', not firing PropertyChanged");
                    }
                    else
                    {
                        logger.DebugFormat("Values are not equal.");
                        logger.DebugFormat("Firing PropertyChanged for {0}.{1}",
                            invocation.InvocationTarget.GetType().Name,
                            propertyName);

                        PropertyChanged(invocation.InvocationTarget, new PropertyChangedEventArgs(propertyName));
                    }

                    return;
                }

            }
        }

        private bool IsPropertyChangedAdd(IInvocation invocation)
        {
            return invocation.Method.Name == "add_PropertyChanged";
        }

        private bool IsPropertyChangedRemove(IInvocation invocation)
        {
            return invocation.Method.Name == "remove_PropertyChanged";
        }

        private bool IsPropertySetter(IInvocation invocation)
        {
            return invocation.Method.IsSpecialName && invocation.Method.Name.StartsWith(SetPrefix);
        }

        private string GetPropertyName(IInvocation invocation)
        {
            return invocation.Method.Name.Substring(SetPrefix.Length);
        }

        private event PropertyChangedEventHandler PropertyChanged = (o, e) => { };
    }
}