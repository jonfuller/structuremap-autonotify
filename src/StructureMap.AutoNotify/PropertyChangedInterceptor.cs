using System.ComponentModel;
using System.Linq;
using Castle.Core.Interceptor;
using log4net;
using StructureMap.AutoNotify.Extensions;

namespace StructureMap.AutoNotify
{
    public class PropertyChangedInterceptor : IInterceptor
    {
        readonly FireOptions _fireOption;
        readonly DependencyMap _dependencyMap;
        static readonly ILog logger = LogManager.GetLogger(typeof(PropertyChangedInterceptor));

        event PropertyChangedEventHandler _propertyChanged = (o, e) => { };
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                _propertyChanged += value;
                logger.DebugFormat("{0} subscribed to an autonotify", value.Target.GetType().Name);
            }
            remove
            {
                _propertyChanged -= value;
                logger.DebugFormat("{0} unsubscribed to an autonotify", value.Target.GetType().Name);
            }
        }

        public PropertyChangedInterceptor(FireOptions fireOption, DependencyMap dependencyMap)
        {
            _fireOption = fireOption;
            _dependencyMap = dependencyMap;
        }

        public void Intercept(IInvocation invocation)
        {
            Interception.Interception.Create(this, invocation, _fireOption, logger).Call();
        }

        public void Notify(IInvocation invocation)
        {
            Notify(invocation.PropertyName(), invocation.InvocationTarget);
        }

        private void Notify(string propertyName, object target)
        {
            logger.DebugFormat("Firing PropertyChanged for {0}.{1}",
                               target.GetType().Name,
                               propertyName);

            _propertyChanged(target, new PropertyChangedEventArgs(propertyName));
        }

        public void SetDependents(IInvocation invocation)
        {
            var propertyName = invocation.PropertyName();

            _dependencyMap
                .Map
                .Where(x => x.SourcePropName.StartsWith(propertyName))
                .Each(propDependency =>
                {
                    propDependency.WasChanged(invocation.InvocationTarget);
                    Notify(propDependency.TargetPropName, invocation.InvocationTarget);
                });
        }
    }
}