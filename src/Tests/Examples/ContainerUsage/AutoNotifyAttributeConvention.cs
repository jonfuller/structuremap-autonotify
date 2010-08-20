using System.ComponentModel;
using NUnit.Framework;
using StructureMap.AutoNotify;
using Tests.Util;
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
}