using System.ComponentModel;
using NUnit.Framework;
using StructureMap.AutoNotify;
using Tests.Util;
using Container = StructureMap.Container;

namespace Tests.Examples.ContainerUsage
{
    [TestFixture]
    public class AutoNotifyFireOptions
    {
        [Test]
        public void UsingFireOptionOnlyOnChange()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With(new AutoNotifyAttrConvention());
                scanConfig.TheCallingAssembly();
                scanConfig.WithDefaultConventions();
            }));

            var foo = container.GetInstance<Foo>();

            var tracker = new EventTracker<PropertyChangedEventHandler>();

            foo.Value = "test";
            (foo as INotifyPropertyChanged).PropertyChanged += tracker;
            foo.Value = "test";

            Assert.That(tracker.WasNotCalled);
        }

        [Test]
        public void UsingFireOptionAlways()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With(new AutoNotifyAttrConvention());
                scanConfig.TheCallingAssembly();
                scanConfig.WithDefaultConventions();
            }));

            var bar = container.GetInstance<Bar>();

            var tracker = new EventTracker<PropertyChangedEventHandler>();

            bar.Value = "test";
            (bar as INotifyPropertyChanged).PropertyChanged += tracker;
            bar.Value = "test";

            Assert.That(tracker.WasCalled);
        }

        [AutoNotify(Fire = FireOptions.OnlyOnChange)]
        public class Foo
        {
            // note for autonotify to work, the property must be virtual
            public virtual string Value { get; set; }
        }

        [AutoNotify(Fire = FireOptions.Always)]
        public class Bar
        {
            // note for autonotify to work, the property must be virtual
            public virtual string Value { get; set; }
        }
    }
}