using System.ComponentModel;
using Castle.DynamicProxy;
using NUnit.Framework;
using StructureMap.AutoNotify;
using Tests.Util;
using Container = StructureMap.Container;

namespace Tests.Examples.ContainerUsage
{
    [TestFixture]
    public class OneOffForClass
    {
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

        public class Bar
        {
            // note for autonotify to work, the property must be virtual
            public virtual string Value { get; set; }
        }
    }
}