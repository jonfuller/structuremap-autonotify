using System;
using StructureMap.Pipeline;

namespace StructureMap.AutoNotify
{
    public class LooseConstructorInstance : Instance
    {
        readonly Func<IContext, object> _builder;

        public LooseConstructorInstance(Func<IContext, object> builder)
        {
            _builder = builder;
        }

        protected override string getDescription()
        {
            return "uses the given builder";
        }

        protected override object build(Type pluginType, BuildSession session)
        {
            return _builder(session);
        }
    }
}