using System.ComponentModel;
using Castle.DynamicProxy;
using NUnit.Framework;
using StructureMap.AutoNotify;
using Tests.Util;

namespace Tests.UnitTests
{
    [TestFixture]
    public class CanMakeNotifiableForConcreteClass
    {
        [TestCase]
        public void ShouldReturnAnINPCForConcreteObjectNoCtorArgs()
        {
            var cat = Notifiable.MakeForClassGeneric<LolCat>(FireOptions.Always, new ProxyGenerator(), new DependencyMap());

            Assert.That(cat, Is.InstanceOf<INotifyPropertyChanged>());
        }

        [TestCase]
        public void ShouldReturnAnINPCForConcreteObjectWithCtorArgs()
        {
            var obj = Notifiable.MakeForClassGeneric<ClassWithDependency>(FireOptions.Always, new ProxyGenerator(), new DependencyMap(), new Dependency { Value = 4 });

            Assert.That(obj, Is.InstanceOf<INotifyPropertyChanged>());
            Assert.That(obj.Dependency.Value, Is.EqualTo(4));
        }

        [TestCase]
        public void ShouldFireChangedWhenVirtualPropertySetOnMadeObject()
        {
            var cat = Notifiable.MakeForClassGeneric<LolCat>(FireOptions.Always, new ProxyGenerator(), new DependencyMap());

            var tracker = new EventTracker<PropertyChangedEventHandler>();

            (cat as INotifyPropertyChanged).PropertyChanged += tracker;

            cat.Greeting = "buzz off";

            Assert.That(tracker.WasCalled);
        }

        [TestCase]
        public void ShouldNotFireChangedWhenVirtualPropertySetAndChangedOnMadeObjectWithChangeOnlyOption()
        {
            var cat = Notifiable.MakeForClassGeneric<LolCat>(FireOptions.OnlyOnChange, new ProxyGenerator(), new DependencyMap());

            var tracker = new EventTracker<PropertyChangedEventHandler>();


            cat.Greeting = "value";
            (cat as INotifyPropertyChanged).PropertyChanged += tracker;
            cat.Greeting = "value";

            Assert.That(tracker.WasNotCalled);
        }

        [TestCase]
        public void ShouldNotFireChangedWhenNonVirtualPropertyChangedOnMadeObject()
        {
            var cat = Notifiable.MakeForClassGeneric<LolCat>(FireOptions.Always, new ProxyGenerator(), new DependencyMap());

            var tracker = new EventTracker<PropertyChangedEventHandler>();

            (cat as INotifyPropertyChanged).PropertyChanged += tracker;

            cat.Color = "purple";

            Assert.That(tracker.WasNotCalled);
        }

        public class LolCat
        {
            public LolCat()
            {
                Greeting = "OHai";
            }

            public string Hello()
            {
                return Greeting;
            }

            public virtual string Greeting { get; set; }
            public string Color { get; set; }
        }

        public class ClassWithDependency
        {
            readonly Dependency _dependency;

            public ClassWithDependency(Dependency dependency)
            {
                _dependency = dependency;
            }

            public Dependency Dependency {get { return _dependency; }}
        }

        public class Dependency
        {
            public int Value { get; set; }
        }
    }
}
