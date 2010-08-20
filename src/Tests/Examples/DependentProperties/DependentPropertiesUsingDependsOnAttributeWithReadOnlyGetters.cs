using System.ComponentModel;
using NUnit.Framework;
using StructureMap.AutoNotify;
using Tests.Util;
using Container = StructureMap.Container;

namespace Tests.Examples.DependentProperties
{
    [TestFixture]
    public class DependentPropertiesUsingDependsOnAttributeWithReadOnlyGetters
    {
        [Test]
        public void UsingDependentPropertiesUpdatesRelatedWhenUsingDependsOnAttribute()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With(new AutoNotifyAttrConvention());
                scanConfig.TheCallingAssembly();
                scanConfig.WithDefaultConventions();
            }));

            var account = container.GetInstance<Account>();
            var projectTracker = new EventTracker<PropertyChangedEventHandler>();

            (account as INotifyPropertyChanged).PropertyChanged += projectTracker;
            account.AccountName = "big customer";
            account.ClientId = "1234";

            Assert.That(projectTracker.CallCount, Is.EqualTo(4));
        }

        [AutoNotify]
        public class Account
        {
            public virtual string AccountName { get; set; }
            public virtual string ClientId { get; set; }

            [DependsOn("AccountName", "ClientId")]
            public virtual string AccountId
            {
                get { return ClientId + "/" + AccountName; }
            }
        }
    }
}
