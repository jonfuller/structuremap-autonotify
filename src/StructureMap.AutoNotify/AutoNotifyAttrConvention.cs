using StructureMap.AutoNotify.Extensions;

namespace StructureMap.AutoNotify
{
    public class AutoNotifyAttrConvention : AutoNotifyPredicateConvention
    {
        public AutoNotifyAttrConvention() :
            base(type => !(!type.IsEnum && type.HasAttribute<AutoNotifyAttribute>()))
        {
        }
    }
}