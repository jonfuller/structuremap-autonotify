using Castle.Core.Interceptor;

namespace StructureMap.AutoNotify.Interception
{
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
}