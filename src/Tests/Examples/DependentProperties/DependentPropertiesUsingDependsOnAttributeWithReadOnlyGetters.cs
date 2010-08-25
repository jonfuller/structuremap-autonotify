using System.Linq;
using System.Collections.Generic;
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
            var propsChanged = new List<string>();

            (account as INotifyPropertyChanged).PropertyChanged += (o, e) => propsChanged.Add(e.PropertyName);
            account.AccountName = "big customer";
            account.ClientId = "1234";

            Assert.That(propsChanged.Count, Is.EqualTo(4));
            Assert.That(propsChanged.Count(x => x == "AccountName"), Is.EqualTo(1));
            Assert.That(propsChanged.Count(x => x == "ClientId"), Is.EqualTo(1));
            Assert.That(propsChanged.Count(x => x == "AccountId"), Is.EqualTo(2));
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
