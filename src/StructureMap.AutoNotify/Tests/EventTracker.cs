using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Tests
{
    public class EventTracker<TDelegate>
        where TDelegate : class
    {
        public static implicit operator TDelegate(EventTracker<TDelegate> tracker)
        {
            return tracker.Delegate;
        }

        public EventTracker()
        {
            var delegateMeta = typeof(TDelegate).GetMethod("Invoke");
            var delegateParams = delegateMeta
                .GetParameters()
                .Select(param => param.ParameterType)
                .Prepend(GetType())
                .ToArray();

            var invokedMethod = GetType().GetMethod("Invoked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            var dynamicHandler = new DynamicMethod("", delegateMeta.ReturnType, delegateParams, GetType(), true);
            var generator = dynamicHandler.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, invokedMethod);
            generator.Emit(OpCodes.Ret);

            Delegate = dynamicHandler.CreateDelegate(typeof(TDelegate), this) as TDelegate;
        }

        public TDelegate Delegate
        {
            get;
            private set;
        }

        public int CallCount
        {
            get;
            private set;
        }

        public bool WasCalled
        {
            get { return CallCount > 0; }
        }

        public bool WasNotCalled
        {
            get { return !WasCalled; }
        }

        // called via the dynamic method
        private void Invoked()
        {
            CallCount++;
        }
    }

    public static class Ext
    {
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> target, T toPrepend)
        {
            yield return toPrepend;
            foreach(var item in target)
                yield return item;
        }
    }
}
