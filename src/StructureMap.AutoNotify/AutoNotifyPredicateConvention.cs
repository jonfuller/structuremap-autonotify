﻿using System;
using System.Linq;
using Castle.DynamicProxy;
using log4net;
using StructureMap.AutoNotify.Extensions;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;

namespace StructureMap.AutoNotify
{
    public class AutoNotifyPredicateConvention : IRegistrationConvention
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(AutoNotifyAttrConvention));

        readonly Func<Type, bool> _shouldAutoNotify;

        public AutoNotifyPredicateConvention(Func<Type, bool> shouldAutoNotify)
        {
            _shouldAutoNotify = shouldAutoNotify;
        }

        public void Process(Type type, Registry registry)
        {
            if(!_shouldAutoNotify(type))
                return;

            logger.InfoFormat("Registering autonotify for {0}", type.Name);

            var fireOption = type.GetAttribute<AutoNotifyAttribute>().Fire;

            var dependencyMap = new DependencyMap()
                .Tap(m => m.Map.AddRange(GetDependencyMap(type.GetAttribute<AutoNotifyAttribute>().DependencyMap).Map))
                .Tap(m => m.Map.AddRange(GetDependencyMapFromProps(type).Map));

            if(type.IsInterface)
                ConfigureInterface(type, registry, fireOption, dependencyMap);
            else if(!type.IsAbstract)
                ConfigureClass(type, registry, fireOption, dependencyMap);
        }

        static DependencyMap GetDependencyMapFromProps(Type type)
        {
            return new DependencyMap().Tap(map =>
            {
                EnumerableExt.Each<ReadOnlyPropertyDependency>(type.GetProperties()
                                                          .Where(prop => prop.HasAttribute<DependsOnAttribute>())
                                                          .Select(prop => new { TargetName = prop.Name, TargetType = prop.PropertyType, prop.GetAttribute<DependsOnAttribute>().DependentProperties })
                                                          .SelectMany(p => p.DependentProperties.Select(x => new { SourceName = x, p.TargetName, p.TargetType }))
                                                          .Select(p => new ReadOnlyPropertyDependency()
                                                          {
                                                              ObjectType = type,
                                                              SourcePropName = p.SourceName,
                                                              SourcePropertyType = null, // TODO: look this up
                                                              TargetPropName = p.TargetName,
                                                              TargetPropertyType = p.TargetType,
                                                          }), p => map.Map.Add(p));
            });
        }

        static DependencyMap GetDependencyMap(Type dependencyMapType)
        {
            if(dependencyMapType == null)
                return new DependencyMap();
            if(!typeof(DependencyMap).IsAssignableFrom(dependencyMapType))
                throw new InvalidOperationException(string.Format("The type {0} is not a valid dependency map.", dependencyMapType.Name));

            return (DependencyMap)Activator.CreateInstance(dependencyMapType);
        }

        private static void ConfigureInterface(Type type, IRegistry registry, FireOptions fireOption, DependencyMap dependencyMap)
        {
            registry
                .For(type)
                .EnrichWith((context, obj) => Notifiable.MakeForInterface(type, obj, fireOption, new ProxyGenerator(), dependencyMap));
        }

        private static void ConfigureClass(Type type, IRegistry registry, FireOptions fireOption, DependencyMap dependencyMap)
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
        }
    }
}