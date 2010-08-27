using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using StructureMap.AutoNotify.Extensions;

namespace StructureMap.AutoNotify
{
    public class DependencyMap<T> : DependencyMap
    {
        public DependencyMapBuilder<T, TSourceProp> Property<TSourceProp>(Expression<Func<T, TSourceProp>> propExpr)
        {
            return new DependencyMapBuilder<T, TSourceProp>(propExpr.Name(), Map);
        }
    }

    public class DependencyMap
    {
        public DependencyMap()
        {
            Map = new List<PropertyDependency>();
        }

        public List<PropertyDependency> Map { get; private set; }
    }

    public class DependencyMapBuilder<TObj, TProp>
    {
        readonly IList<PropertyDependency> _map;

        readonly string _givenPropName;

        public DependencyMapBuilder(string givenPropName, IList<PropertyDependency> map)
        {
            _givenPropName = givenPropName;
            _map = map;
        }

        /// <summary>
        /// Property(x => x.FirstName).Updates(x => x.FullName).With(x => x.FirstName + " " + x.LastName)
        /// </summary>
        /// <remarks>In this case, the original property is the source property.</remarks>
        public UpdatesBuilder<TObj, TProp, TTargetProp> Updates<TTargetProp>(Expression<Func<TObj, TTargetProp>> targetPropExpr)
        {
            var dependency = new ReadOnlyPropertyDependency()
            {
                SourcePropName = _givenPropName,
                TargetPropName = targetPropExpr.Name(),
                ObjectType = typeof(TObj),
                SourcePropertyType = typeof(TProp),
                TargetPropertyType = typeof(TTargetProp),
            };
            _map.Add(dependency);

            return new UpdatesBuilder<TObj, TProp, TTargetProp>(_givenPropName, targetPropExpr.Name(), _map, dependency);
        }

        /// <summary>
        /// Property(x => x.FullName).DependsOn(x => x.FirstName);
        /// </summary>
        /// <remarks>In this case, the original property is the target property.</remarks>
        public void DependsOn<TSourceProp>(Expression<Func<TObj, TSourceProp>> sourcePropExpr)
        {
            _map.Add(new ReadOnlyPropertyDependency()
            {
                ObjectType = typeof(TObj),
                SourcePropertyType = typeof(TSourceProp),
                SourcePropName = sourcePropExpr.Name(),
                TargetPropertyType = typeof(TProp),
                TargetPropName = _givenPropName
            });
        }
    }

    public class UpdatesBuilder<TObj, TSourceProp, TTargetProp>
    {
        readonly string _sourcePropName;
        readonly string _targetPropName;
        readonly IList<PropertyDependency> _map;
        readonly ReadOnlyPropertyDependency _dependency;

        public UpdatesBuilder(string sourcePropName, string targetPropName, IList<PropertyDependency> map, ReadOnlyPropertyDependency dependency)
        {
            _sourcePropName = sourcePropName;
            _targetPropName = targetPropName;
            _map = map;
            _dependency = dependency;
        }

        public void With(Func<TObj, TTargetProp> setter)
        {
            _map.Remove(_dependency);

            _map.Add(new WritingPropertyDependency
            {
                SourcePropName = _sourcePropName,
                TargetPropName = _targetPropName,
                ObjectType = typeof(TObj),
                SourcePropertyType = typeof(TSourceProp),
                TargetPropertyType = typeof(TTargetProp),
                NewValue = o => setter((TObj)o),
            });
        }
    }

    public class ReadOnlyPropertyDependency : PropertyDependency
    {
        public override void WasChanged(object target){}
    }

    public class WritingPropertyDependency : PropertyDependency
    {
        public Func<object, object> NewValue { get; set; }

        public override void WasChanged(object target)
        {
            // the target should be a proxy, so it should ALWAYS be safe to use the base type
            var setter = target.GetType().BaseType.GetProperty(TargetPropName).GetSetMethod(true);

            setter.Invoke(target, new[] { NewValue(target) });
        }
    }

    public abstract class PropertyDependency
    {
        public Type ObjectType { get; set; }
        public Type SourcePropertyType { get; set; }
        public string SourcePropName { get; set; }

        public string TargetPropName { get; set; }

        public Type TargetPropertyType { get; set; }

        public abstract void WasChanged(object target);
    }
}