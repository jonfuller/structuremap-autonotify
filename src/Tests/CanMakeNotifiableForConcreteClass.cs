using System.ComponentModel;
using Castle.DynamicProxy;
using NUnit.Framework;
using StructureMap.AutoNotify;

namespace Tests
{
    [TestFixture]
    public class CanMakeNotifiableForConcreteClass
    {
        [TestCase]
        public void ShouldReturnAnINPCForConcreteObjectNoCtorArgs()
        {
            var cat = Notifiable.MakeForClassGeneric<LolCat>(new ProxyGenerator());

            Assert.That(cat, Is.InstanceOf<INotifyPropertyChanged>());
        }

        [TestCase]
        public void ShouldReturnAnINPCForConcreteObjectWithCtorArgs()
        {
            var obj = Notifiable.MakeForClassGeneric<ClassWithDependency>(new ProxyGenerator(), new Dependency{Value = 4});

            Assert.That(obj, Is.InstanceOf<INotifyPropertyChanged>());
            Assert.That(obj.Dependency.Value, Is.EqualTo(4));
        }

        [TestCase]
        public void ShouldFireChangedWhenVirtualPropertyChangedOnMadeObject()
        {
            var cat = Notifiable.MakeForClassGeneric<LolCat>(new ProxyGenerator());

            var tracker = new EventTracker<PropertyChangedEventHandler>();

            (cat as INotifyPropertyChanged).PropertyChanged += tracker;

            cat.Greeting = "buzz off";

            Assert.That(tracker.WasCalled);
        }

        [TestCase]
        public void ShouldNotFireChangedWhenNonVirtualPropertyChangedOnMadeObject()
        {
            var cat = Notifiable.MakeForClassGeneric<LolCat>(new ProxyGenerator());

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
