using System.ComponentModel;
using Castle.DynamicProxy;
using NUnit.Framework;
using StructureMap.AutoNotify;
using Tests.Util;
using Container = StructureMap.Container;

namespace Tests.Examples.ContainerUsage
{
    [TestFixture]
    public class OneOffForInterface
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

        public interface IFoo
        {
            string Value { get; set; }
        }

        public class Foo : IFoo
        {
            public string Value { get; set; }
        }
    }
}