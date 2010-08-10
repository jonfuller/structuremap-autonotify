﻿using System.ComponentModel;
using Castle.Core.Interceptor;

namespace StructureMap.AutoNotify
{
    public class PropertyChangedDecorator : IInterceptor
    {
        const string SetPrefix = "set_";

        public void Intercept(IInvocation invocation)
        {
            if(IsPropertyChangedAdd(invocation))
            {
                PropertyChanged += (PropertyChangedEventHandler)invocation.GetArgumentValue(0);
                return;
            }
            if(IsPropertyChangedRemove(invocation))
            {
                PropertyChanged -= (PropertyChangedEventHandler)invocation.GetArgumentValue(0);
                return;
            }

            invocation.Proceed();
            if(IsPropertySetter(invocation))
            {
                PropertyChanged(invocation.InvocationTarget, new PropertyChangedEventArgs(GetPropertyName(invocation)));
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