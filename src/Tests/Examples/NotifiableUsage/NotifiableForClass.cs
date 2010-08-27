using System.ComponentModel;
using Castle.DynamicProxy;
using NUnit.Framework;
using StructureMap.AutoNotify;

namespace Tests.Examples.NotifiableUsage
{
    [TestFixture]
    public class NotifiableForClass
    {
        [Test]
        public void CanMakeAnObjectNotifiableWithClass()
        {
            var notifiableFoo = Notifiable.MakeForClass(typeof(Foo), FireOptions.Always, new object[0], new ProxyGenerator(), DependencyMap.Empty);

            Assert.That(notifiableFoo is INotifyPropertyChanged);
        }

        public class Foo
        {
            public virtual string Value { get; set; }
        }
    }
}
