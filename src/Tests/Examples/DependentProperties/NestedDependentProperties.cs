using System.ComponentModel;
using NUnit.Framework;
using StructureMap.AutoNotify;
using Tests.Util;
using Container = StructureMap.Container;

namespace Tests.Examples.DependentProperties
{
    [TestFixture]
    public class NestedDependendProperties
    {
        [Test]
        public void WhenPropertyIsINotifyFirePropertyChangedWhenOneOfItsMembersChange()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With(new AutoNotifyAttrConvention());
                scanConfig.TheCallingAssembly();
                scanConfig.WithDefaultConventions();
            }));

            var contact = container.GetInstance<Contact>();
            var address = container.GetInstance<Address>();

            contact.Address = address;

            var tracker = new EventTracker<PropertyChangedEventHandler>();
            (contact as INotifyPropertyChanged).PropertyChanged += tracker;

            contact.Address.Street1 = "1600 Pennsylvania";

            Assert.That(tracker.WasCalled);
        }

        [Test]
        public void INotifyDoesntGetFiredForPreviouslySetINotifyProperty()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With(new AutoNotifyAttrConvention());
                scanConfig.TheCallingAssembly();
                scanConfig.WithDefaultConventions();
            }));

            var contact = container.GetInstance<Contact>();
            var address1 = container.GetInstance<Address>();
            var address2 = container.GetInstance<Address>();

            contact.Address = address1;

            var tracker = new EventTracker<PropertyChangedEventHandler>();
            (contact as INotifyPropertyChanged).PropertyChanged += tracker;

            contact.Address.Zip = "46032";  // fire once
             
            contact.Address = address2;     // second fire

            contact.Address.Zip = "46033";  // third fire

            address1.Zip = "46034";         // shouldn't fire

            Assert.That(tracker.CallCount, Is.EqualTo(3));
        }

        [Test]
        public void INotifyIsFiredForParentPropertyWhenNestedPropertyIsChanged()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With(new AutoNotifyAttrConvention());
                scanConfig.TheCallingAssembly();
                scanConfig.WithDefaultConventions();
            }));

            var contact = container.GetInstance<Contact>();
            var address = container.GetInstance<Address>();

            contact.Address = address;

            var changedProp = string.Empty;
            (contact as INotifyPropertyChanged).PropertyChanged += (o, e) => changedProp = e.PropertyName;

            contact.Address.Street1 = "1600 Pennsylvania";

            Assert.That(changedProp, Is.EqualTo("Address"));
        }

        [Test]
        public void NotifyIsFiredForDependentPropertyWhenSourcePropertyIsANestedProperty()
        {

        }

        [AutoNotify]
        public class Contact
        {
            public virtual string FirstName { get; set; }
            public virtual string LastName { get; set; }
            public virtual Address Address { get; set; }
            
        }

        [AutoNotify]
        public class Address
        {
            public virtual string Street1 { get; set; }
            public virtual string Street2 { get; set; }
            public virtual string City { get; set; }
            public virtual string State { get; set; }
            public virtual string Zip { get; set; }
        }
    }
}
