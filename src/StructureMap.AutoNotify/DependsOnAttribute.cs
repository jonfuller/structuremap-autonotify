using System;

namespace StructureMap.AutoNotify
{
    public class DependsOnAttribute : Attribute
    {
        public string[] DependentProperties { get; private set; }

        public DependsOnAttribute(params string[] dependentProperties)
        {
            DependentProperties = dependentProperties;
        }
    }
}