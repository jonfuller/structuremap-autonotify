using System;
using System.Linq;
using Castle.DynamicProxy;
using log4net;
using StructureMap.Graph;

namespace StructureMap.AutoNotify
{
    public class AutoNotifyScanner : ITypeScanner
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(AutoNotifyScanner));

        public void Process(Type type, PluginGraph graph)
        {
            if(type.IsEnum || !type.HasAttribute<AutoNotifyAttribute>())
                return;

            logger.InfoFormat("Registering autonotify for {0}", type.Name);

            var fireOption = type.GetAttribute<AutoNotifyAttribute>().Fire;
            var dependencyMap = GetDependencyMap(type.GetAttribute<AutoNotifyAttribute>().DependencyMap);

            if(type.IsInterface)
                ConfigureInterface(type, graph, fireOption, dependencyMap);
            else if(!type.IsAbstract)
                ConfigureClass(type, graph, fireOption, dependencyMap);
        }

        static DependencyMap GetDependencyMap(Type dependencyMapType)
        {
            if (dependencyMapType == null)
                return new DependencyMap();
            if (!typeof(DependencyMap).IsAssignableFrom(dependencyMapType))
                throw new InvalidOperationException(string.Format("The type {0} is not a valid dependency map.", dependencyMapType.Name));

            return (DependencyMap)Activator.CreateInstance(dependencyMapType);
        }

        private void ConfigureInterface(Type type, PluginGraph graph, FireOptions fireOption, DependencyMap dependencyMap)
        {
            graph.Configure(registry =>
            {
                registry
                    .For(type)
                    .EnrichWith((context, obj) => Notifiable.MakeForInterface(type, obj, fireOption, new ProxyGenerator(), dependencyMap));
            });
        }

        private void ConfigureClass(Type type, PluginGraph graph, FireOptions fireOption, DependencyMap dependencyMap)
        {
            graph.Configure(registry =>
            {
                var inst = new LooseConstructorInstance(context =>
                {
                    var ctorArgs = type
                        .GetGreediestCtor()
                        .GetParameters()
                        .Select(p => context.GetInstance(p.ParameterType));

                    return Notifiable.MakeForClass(type, fireOption, ctorArgs.ToArray(), new ProxyGenerator(), dependencyMap);
                });

                registry.For(type).Use(inst);
            });
        }
    }
}