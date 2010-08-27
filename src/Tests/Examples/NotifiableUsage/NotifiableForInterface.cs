using System.ComponentModel;
using Castle.DynamicProxy;
using NUnit.Framework;
using StructureMap.AutoNotify;

namespace Tests.Examples.NotifiableUsage
{
    [TestFixture]
    public class NotifiableForInterface
    {
        [Test]
        public void CanMakeAnObjectNotifiableWithInterface()
        {
            var foo = new Foo();
            var notifiableFoo = Notifiable.MakeForInterface(typeof(IFoo), foo, FireOptions.Always, new ProxyGenerator(), DependencyMap.Empty);

            Assert.That(notifiableFoo is INotifyPropertyChanged);
        }

        public interface IFoo
        {
            string Value { get; set; }
        }

        internal class Foo : IFoo
        {
            public string Value { get; set; }
        }
    }
}
