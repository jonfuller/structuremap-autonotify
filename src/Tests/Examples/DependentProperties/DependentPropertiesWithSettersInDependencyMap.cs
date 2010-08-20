using System.ComponentModel;
using NUnit.Framework;
using StructureMap.AutoNotify;
using Tests.Util;
using Container = StructureMap.Container;

namespace Tests.Examples.DependentProperties
{
    [TestFixture]
    public class DependentPropertiesWithSettersInDependencyMap
    {
        [Test]
        public void UsingDependentPropertiesUpdatesRelatedProps()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With(new AutoNotifyAttrConvention());
                scanConfig.TheCallingAssembly();
                scanConfig.WithDefaultConventions();
            }));

            var book = container.GetInstance<Book>();
            var bookTracker = new EventTracker<PropertyChangedEventHandler>();

            (book as INotifyPropertyChanged).PropertyChanged += bookTracker;
            book.Authors = new[] { "Stephen King", "Jon Fuller" };

            Assert.That(book.AuthorCount, Is.EqualTo(2));
        }

        [Test]
        public void UsingDependentPropertiesFiresNotifyForAllRelatedProps()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With(new AutoNotifyAttrConvention());
                scanConfig.TheCallingAssembly();
                scanConfig.WithDefaultConventions();
            }));

            var book = container.GetInstance<Book>();
            var bookTracker = new EventTracker<PropertyChangedEventHandler>();

            (book as INotifyPropertyChanged).PropertyChanged += bookTracker;
            book.Authors = new[] { "Stephen King", "Jon Fuller" };

            Assert.That(bookTracker.CallCount, Is.EqualTo(2));
        }

        [Test]
        public void UsingDependentPropertiesUpdatesRelatedWhenChangedOnFireOnlyChange()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With(new AutoNotifyAttrConvention());
                scanConfig.TheCallingAssembly();
                scanConfig.WithDefaultConventions();
            }));

            var contact = container.GetInstance<Contact>();
            var bookTracker = new EventTracker<PropertyChangedEventHandler>();

            contact.FirstName = "Jon";
            contact.LastName = "Fuller";
            (contact as INotifyPropertyChanged).PropertyChanged += bookTracker;
            contact.FirstName = "Jonathon";

            Assert.That(contact.Name, Is.EqualTo("Jonathon Fuller"));
        }

        [AutoNotify(DependencyMap = typeof(BookDependency))]
        public class Book
        {
            public virtual string[] Authors { get; set; }
            public virtual string Title { get; set; }
            public virtual int AuthorCount { get; private set; }
        }

        class BookDependency : DependencyMap<Book>
        {
            public BookDependency()
            {
                Property(x => x.Authors).Updates(x => x.AuthorCount).With(x => x.Authors.Length);
            }
        }

        [AutoNotify(Fire = FireOptions.OnlyOnChange, DependencyMap = typeof(ContactDependency))]
        public class Contact
        {
            public virtual string FirstName { get; set; }
            public virtual string LastName { get; set; }
            public virtual string Name { get; private set; }
        }

        class ContactDependency : DependencyMap<Contact>
        {
            public ContactDependency()
            {
                Property(x => x.FirstName).Updates(x => x.Name).With(UpdateName);
                Property(x => x.LastName).Updates(x => x.Name).With(UpdateName);
            }

            static string UpdateName(Contact contact)
            {
                return contact.FirstName + " " + contact.LastName;
            }
        }
    }
}