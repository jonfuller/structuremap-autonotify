using System.ComponentModel;
using Castle.Core.Interceptor;
using log4net;

namespace StructureMap.AutoNotify
{
    public class PropertyChangedDecorator : IInterceptor
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(PropertyChangedDecorator));
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

            invocation.Proceed();
            if(IsPropertySetter(invocation))
            {
                var propertyName = GetPropertyName(invocation);

                logger.DebugFormat("Firing PropertyChanged for {0}.{1}",
                    invocation.InvocationTarget.GetType().Name,
                    propertyName);

                PropertyChanged(invocation.InvocationTarget, new PropertyChangedEventArgs(propertyName));
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