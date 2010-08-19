using System;

namespace StructureMap.AutoNotify
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class AutoNotifyAttribute : Attribute
    {
        public AutoNotifyAttribute()
        {
            Fire = FireOptions.Always;
        }

        /// <summary>
        /// Under what circumstances to fire the event.
        /// </summary>
        /// <remarks>Defaults to <see cref="FireOptions.Always"/>.</remarks>
        public FireOptions Fire { get; set; }

        /// <summary>
        /// A type that inherits from DependencyMap{T} or DependencyMap, used for setting up dependent properties.
        /// </summary>
        public Type DependencyMap { get; set; }
    }

    public enum FireOptions
    {
        /// <summary>
        /// Fire the event any time a setter is called.
        /// </summary>
        Always,

        /// <summary>
        /// Fire the event only when the given value is different than the previous value.
        /// </summary>
        OnlyOnChange
    }
}