using System.ComponentModel;
using System.Linq;
using Castle.Core.Interceptor;
using log4net;
using StructureMap.AutoNotify.Extensions;

namespace StructureMap.AutoNotify
{
    public class PropertyChangedDecorator : IInterceptor
    {
        readonly FireOptions _fireOption;
        readonly DependencyMap _dependencyMap;
        static readonly ILog logger = LogManager.GetLogger(typeof(PropertyChangedDecorator));

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

        public PropertyChangedDecorator(FireOptions fireOption, DependencyMap dependencyMap)
        {
            _fireOption = fireOption;
            _dependencyMap = dependencyMap;
        }

        public void Intercept(IInvocation invocation)
        {
            WrapInvocation(this, invocation, _fireOption, logger).Call();
        }

        static IInterception WrapInvocation(PropertyChangedDecorator propertyChangedDecorator, IInvocation invocation, FireOptions fireOption, ILog log)
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

        public void Notify(IInvocation invocation)
        {
            var propertyName = invocation.PropertyName();

            logger.DebugFormat("Firing PropertyChanged for {0}.{1}",
                               invocation.InvocationTarget.GetType().Name,
                               propertyName);

            _propertyChanged(invocation.InvocationTarget, new PropertyChangedEventArgs(propertyName));
        }

        public void SetDependents(IInvocation invocation)
        {
            var propertyName = invocation.PropertyName();

            _dependencyMap.Map.Where(x => x.SourcePropName == propertyName).Each(propDependency =>
            {
                var target = invocation.InvocationTarget;

                if(propDependency is WritingPropertyDependency)
                {
                    var writingDep = propDependency as WritingPropertyDependency;
                    var setter = target.GetType().BaseType.GetProperty(propDependency.TargetPropName).GetSetMethod(true);
                    var newValue = writingDep.Setter(target);

                    setter.Invoke(target, new[] { newValue });
                }
                Notify(invocation);
            });
        }
    }

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

    class InvocationInterception : IInterception
    {
        readonly IInvocation _invocation;

        public InvocationInterception(IInvocation invocation)
        {
            _invocation = invocation;
        }

        public void Call()
        {
            _invocation.Proceed();
        }
    }

    class OnlyOnChangePropertySetterInterception : IInterception
    {
        readonly PropertyChangedDecorator _propertyChangedDecorator;
        readonly IInvocation _invocation;
        readonly string _propertyName;
        readonly ILog _logger;

        public OnlyOnChangePropertySetterInterception(PropertyChangedDecorator propertyChangedDecorator, IInvocation invocation, string propertyName, ILog logger)
        {
            _propertyChangedDecorator = propertyChangedDecorator;
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
            _propertyChangedDecorator.Notify(_invocation);
            _propertyChangedDecorator.SetDependents(_invocation);
        }

        private static bool AreEqual(object oldValue, object newValue)
        {
            return (oldValue == null && newValue == null)
                   || (oldValue != null && oldValue.Equals(newValue))
                   || (newValue != null && newValue.Equals(oldValue));
        }
    }

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

    class PropertyChangedAddInterception : IInterception
    {
        readonly PropertyChangedDecorator _propertyChangedDecorator;
        readonly IInvocation _invocation;

        public PropertyChangedAddInterception(PropertyChangedDecorator propertyChangedDecorator, IInvocation invocation)
        {
            _propertyChangedDecorator = propertyChangedDecorator;
            _invocation = invocation;
        }

        public void Call()
        {
            var onPropertyChanged = (PropertyChangedEventHandler)_invocation.GetArgumentValue(0);
            _propertyChangedDecorator.PropertyChanged += onPropertyChanged;
        }
    }

    interface IInterception
    {
        void Call();
    }
}