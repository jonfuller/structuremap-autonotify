using System.ComponentModel;
using NUnit.Framework;
using StructureMap.AutoNotify;
using Container = StructureMap.Container;

namespace Tests.UnitTests
{
    [TestFixture]
    public class AutoNotifyScannerTests
    {
        [TestCase]
        public void ShouldPickUpAutoNotifyAttributedClass()
        {
            var container = new Container(config => config.Scan(scan =>
            {
                scan.With<AutoNotifyAttrConvention>();
                scan.TheCallingAssembly();
            }));

            Assert.That(container.GetInstance<TestClassWithAttr>(), Is.InstanceOf<INotifyPropertyChanged>());
        }

        [TestCase]
        public void ShouldNotPickUpNonAttributedClass()
        {
            var container = new Container(config => config.Scan(scan =>
            {
                scan.With<AutoNotifyAttrConvention>();
                scan.TheCallingAssembly();
            }));

            Assert.That(container.GetInstance<TestClassWithoutAttr>(), Is.Not.InstanceOf<INotifyPropertyChanged>());
        }
    }

    [AutoNotify]
    public class TestClassWithAttr
    { }

    public class TestClassWithoutAttr
    { }
}