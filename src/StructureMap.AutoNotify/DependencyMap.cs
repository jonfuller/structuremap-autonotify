using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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

    public class PropertyDependency
    {
        public Type ObjectType { get; set; }
        public Type SourcePropertyType { get; set; }
        public string SourcePropName { get; set; }
        public Func<object, object> Setter { get; set; }

        public string TargetPropName { get; set; }

        public Type TargetPropertyType { get; set; }
    }

    public class DependencyMapBuilder<TObj, TSourceProp>
    {
        readonly IList<PropertyDependency> _map;

        readonly string _sourcePropName;

        public DependencyMapBuilder(string sourcePropName, IList<PropertyDependency> map)
        {
            _sourcePropName = sourcePropName;
            _map = map;
        }

        public DependencyMapBuilder<TObj, TSourceProp, TTargetProp> ShouldUpdate<TTargetProp>(Expression<Func<TObj, TTargetProp>> targetPropExpr)
        {
            return new DependencyMapBuilder<TObj, TSourceProp, TTargetProp>(_sourcePropName, targetPropExpr.Name(), _map);
        }
    }

    public class DependencyMapBuilder<TObj, TSourceProp, TTargetProp>
    {
        readonly string _sourcePropName;
        readonly string _targetPropName;
        readonly IList<PropertyDependency> _map;

        public DependencyMapBuilder(string sourcePropName, string targetPropName, IList<PropertyDependency> map)
        {
            _sourcePropName = sourcePropName;
            _targetPropName = targetPropName;
            _map = map;
        }

        public void With(Func<TObj, TTargetProp> setter)
        {
            _map.Add(new PropertyDependency
            {
                SourcePropName = _sourcePropName,
                TargetPropName = _targetPropName,
                ObjectType = typeof(TObj),
                SourcePropertyType = typeof(TSourceProp),
                TargetPropertyType = typeof(TTargetProp),
                Setter = o => setter((TObj)o),
            });
        }
    }
}