using System.ComponentModel;
using Castle.DynamicProxy;
using NUnit.Framework;
using StructureMap.AutoNotify;
using Tests.Util;
using Container = StructureMap.Container;

namespace Tests.Examples
{
    [TestFixture]
    public class ContainerUsage
    {
        [TestCase]
        public void EnrichInterface()
        {
            var container = new Container(config =>
            {
                config
                    .For<IFoo>()
                    .EnrichAllWith((context, obj) => Notifiable.MakeForInterfaceGeneric(obj, FireOptions.Always, new ProxyGenerator(), new DependencyMap()))
                    .Use<Foo>();
            });

            var foo = container.GetInstance<IFoo>();

            // make sure it's wrapped
            Assert.That(foo, Is.InstanceOf<INotifyPropertyChanged>());

            // make sure it fires properly
            var tracker = new EventTracker<PropertyChangedEventHandler>();

            (foo as INotifyPropertyChanged).PropertyChanged += tracker;
            foo.Value = "yo";

            Assert.That(tracker.WasCalled);
        }

        [TestCase]
        public void CreateForClass()
        {
            var container = new Container(config =>
            {
                config
                    .For<Bar>()
                    .Use(context => Notifiable.MakeForClassGeneric<Bar>(FireOptions.Always, new ProxyGenerator(), new DependencyMap()));
            });

            var bar = container.GetInstance<Bar>();

            // make sure it's wrapped
            Assert.That(bar, Is.InstanceOf<INotifyPropertyChanged>());


            // make sure it fires properly
            var tracker = new EventTracker<PropertyChangedEventHandler>();

            (bar as INotifyPropertyChanged).PropertyChanged += tracker;
            bar.Value = "yo";

            Assert.That(tracker.WasCalled);
        }

        [TestCase]
        public void UsingAutoNotifyAttributeConvention()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With<AutoNotifyAttrConvention>();
                scanConfig.TheCallingAssembly();
                scanConfig.WithDefaultConventions();
            }));

            var gem = container.GetInstance<IGem>();
            var rock = container.GetInstance<Rock>();


            Assert.That(gem, Is.InstanceOf<INotifyPropertyChanged>());
            Assert.That(rock, Is.InstanceOf<INotifyPropertyChanged>());


            // Make sure "FireOptions.OnlyOnChange" got hooked up correctly for Rock
            var rockTracker = new EventTracker<PropertyChangedEventHandler>();

            rock.Value = "test";
            (rock as INotifyPropertyChanged).PropertyChanged += rockTracker;
            rock.Value = "test";

            Assert.That(rockTracker.WasNotCalled);
        }
    }

    public interface IFoo
    {
        string Value { get; set; }
    }

    public class Foo : IFoo
    {
        public string Value { get; set; }
    }

    public class Bar
    {
        // note for autonotify to work, the property must be virtual
        public virtual string Value { get; set; }
    }

    // note the autonotify attribute goes on the interface, not the class
    [AutoNotify]
    public interface IGem
    {
        string Value { get; set; }
    }

    public class Gem : IGem
    {
        public string Value { get; set; }
    }

    [AutoNotify(Fire = FireOptions.OnlyOnChange)]
    public class Rock
    {
        // note for autonotify to work, the property must be virtual
        public virtual string Value { get; set; }
    }
}