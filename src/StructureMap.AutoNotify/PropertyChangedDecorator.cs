using System;
using System.ComponentModel;
using System.Linq;
using Castle.Core.Interceptor;
using log4net;

namespace StructureMap.AutoNotify
{
    public class PropertyChangedDecorator : IInterceptor
    {
        const string SetPrefix = "set_";

        readonly FireOptions _fireOption;
        readonly DependencyMap _dependencyMap;
        static readonly ILog logger = LogManager.GetLogger(typeof(PropertyChangedDecorator));

        public PropertyChangedDecorator(FireOptions fireOption, DependencyMap dependencyMap)
        {
            _fireOption = fireOption;
            _dependencyMap = dependencyMap;
        }


        public void Intercept(IInvocation invocation)
        {
            WrapInvocation(this, invocation, _fireOption, logger).Call();
        }

        IToCall WrapInvocation(PropertyChangedDecorator propertyChangedDecorator, IInvocation invocation, FireOptions fireOption, ILog log)
        {
            if(IsPropertyChangedAdd(invocation))
                return new PropertyChangedAddToCall(propertyChangedDecorator, invocation);
            if(IsPropertyChangedRemove(invocation))
                return new PropertyChangedRemoveToCall(propertyChangedDecorator, invocation);
            if(IsPropertySetter(invocation) && FireOptions.OnlyOnChange == fireOption)
                return new OnlyOnChangePropertySetterToCall(propertyChangedDecorator, invocation, GetPropertyName(invocation), log);
            if(IsPropertySetter(invocation))
                return new PropertySetterToCall(propertyChangedDecorator, invocation);
            return new InvocationToCall(invocation);
        }

        public void SetDependents(IInvocation invocation)
        {
            var propertyName = GetPropertyName(invocation);

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

        public void Notify(IInvocation invocation)
        {
            var propertyName = GetPropertyName(invocation);

            logger.DebugFormat("Firing PropertyChanged for {0}.{1}",
                               invocation.InvocationTarget.GetType().Name,
                               propertyName);

            _propertyChanged(invocation.InvocationTarget, new PropertyChangedEventArgs(propertyName));
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
        event PropertyChangedEventHandler _propertyChanged = (o, e) => { };
    }

    class PropertySetterToCall : IToCall
    {
        readonly PropertyChangedDecorator _propertyChangedDecorator;
        readonly IInvocation _invocation;

        public PropertySetterToCall(PropertyChangedDecorator propertyChangedDecorator, IInvocation invocation)
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

    class InvocationToCall : IToCall
    {
        readonly IInvocation _invocation;

        public InvocationToCall(IInvocation invocation)
        {
            _invocation = invocation;
        }

        public void Call()
        {
            _invocation.Proceed();
        }
    }

    class OnlyOnChangePropertySetterToCall : IToCall
    {
        readonly PropertyChangedDecorator _propertyChangedDecorator;
        readonly IInvocation _invocation;
        readonly string _propertyName;
        readonly ILog _logger;

        public OnlyOnChangePropertySetterToCall(PropertyChangedDecorator propertyChangedDecorator, IInvocation invocation, string propertyName, ILog logger)
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

    class PropertyChangedRemoveToCall : IToCall
    {
        readonly PropertyChangedDecorator _propertyChangedDecorator;
        readonly IInvocation _invocation;

        public PropertyChangedRemoveToCall(PropertyChangedDecorator propertyChangedDecorator, IInvocation invocation)
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

    class PropertyChangedAddToCall : IToCall
    {
        readonly PropertyChangedDecorator _propertyChangedDecorator;
        readonly IInvocation _invocation;

        public PropertyChangedAddToCall(PropertyChangedDecorator propertyChangedDecorator, IInvocation invocation)
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

    interface IToCall
    {
        void Call();
    }
}