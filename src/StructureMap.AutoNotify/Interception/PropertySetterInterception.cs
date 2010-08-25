using System.Collections.Generic;
using System.ComponentModel;
using Castle.Core.Interceptor;
using StructureMap.AutoNotify.Extensions;

namespace StructureMap.AutoNotify.Interception
{
    class WrappingInterception : IInterception
    {
        readonly IWrappingInterception _wrapper;
        readonly IInterception _toWrap;

        public WrappingInterception(IWrappingInterception wrapper, IInterception toWrap)
        {
            _wrapper = wrapper;
            _toWrap = toWrap;
        }

        public void Call()
        {
            _wrapper.Before();
            _toWrap.Call();
            _wrapper.After();
        }
    }

    public interface IWrappingInterception
    {
        void Before();
        void After();
    }

    class PropertyIsINotifyInterception : IWrappingInterception
    {
        static readonly Dictionary<object, Dictionary<string, PropertyChangedEventHandler>> handlers = new Dictionary<object, Dictionary<string, PropertyChangedEventHandler>>();
        readonly PropertyChangedInterceptor _propertyChangedInterceptor;
        readonly IInvocation _invocation;

        public PropertyIsINotifyInterception(PropertyChangedInterceptor propertyChangedInterceptor, IInvocation invocation)
        {
            _propertyChangedInterceptor = propertyChangedInterceptor;
            _invocation = invocation;
        }

        public void Before()
        {
            if(!(_invocation.GetCurrentValue() is INotifyPropertyChanged))
                return;
            if(_invocation.GetCurrentValue() == null)
                return;

            RemoveHandler(_invocation);
        }

        public void After()
        {
            if(!(_invocation.GetArgumentValue(0) is INotifyPropertyChanged))
                return;
            if(_invocation.GetArgumentValue(0) == null)
                return;

            AddHandler(_invocation, _propertyChangedInterceptor);
        }

        static void AddHandler(IInvocation invocation, PropertyChangedInterceptor propertyChangedInterceptor)
        {
            if (!handlers.ContainsKey(invocation.InvocationTarget))
                handlers.Add(invocation.InvocationTarget, new Dictionary<string, PropertyChangedEventHandler>());

            handlers[invocation.InvocationTarget].Add(invocation.PropertyName(), (o, e) =>
            {
                propertyChangedInterceptor.Notify(invocation);
                propertyChangedInterceptor.SetDependents(invocation);
            });

            (invocation.GetArgumentValue(0) as INotifyPropertyChanged).PropertyChanged += handlers[invocation.InvocationTarget][invocation.PropertyName()];
        }

        static void RemoveHandler(IInvocation invocation)
        {
            (invocation.GetCurrentValue() as INotifyPropertyChanged).PropertyChanged -= handlers[invocation.InvocationTarget][invocation.PropertyName()];
            handlers.Remove(invocation.InvocationTarget);
        }
    }

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