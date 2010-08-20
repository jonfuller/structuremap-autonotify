using System.ComponentModel;
using NUnit.Framework;
using StructureMap.AutoNotify;
using Container = StructureMap.Container;

namespace Tests.Examples.ContainerUsage
{
    [TestFixture]
    public class AutoNotifyAttributeConvention
    {
        [TestCase]
        public void UsingAutoNotifyAttributeConvention()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With(new AutoNotifyAttrConvention());
                scanConfig.TheCallingAssembly();
                scanConfig.WithDefaultConventions();
            }));

            var gem = container.GetInstance<IGem>();

            Assert.That(gem, Is.InstanceOf<INotifyPropertyChanged>());
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
    }
}