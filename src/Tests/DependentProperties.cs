using System.ComponentModel;
using NUnit.Framework;
using StructureMap.AutoNotify;
using Container = StructureMap.Container;

namespace Tests
{
    [TestFixture]
    public class DependentProperties
    {
        [Test]
        public void UsingDependentPropertiesUpdatesRelatedProps()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With<AutoNotifyScanner>();
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
                scanConfig.With<AutoNotifyScanner>();
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
                scanConfig.With<AutoNotifyScanner>();
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
    }

    [AutoNotify(DependencyMap = typeof(BookDependency))]
    public class Book
    {
        public virtual string[] Authors { get; set; }
        public virtual string Title { get; set; }
        public virtual int AuthorCount { get; private set; }
    }

    public class BookDependency : DependencyMap<Book>
    {
        public BookDependency()
        {
            Property(x => x.Authors).ShouldUpdate(x => x.AuthorCount).With(x => x.Authors.Length);
        }
    }

    [AutoNotify(Fire = FireOptions.OnlyOnChange, DependencyMap = typeof(ContactDependency))]
    public class Contact
    {
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Name { get; private set; }

        //[DependsOn("FirstName", "LastName")]
        //public virtual string Name2
        //{
        //    get { return FirstName + " " + LastName; }
        //}
    }

    public class ContactDependency : DependencyMap<Contact>
    {
        public ContactDependency()
        {
            Property(x => x.FirstName).ShouldUpdate(x => x.Name).With(UpdateName);
            Property(x => x.LastName).ShouldUpdate(x => x.Name).With(UpdateName);

            For(x => x.Name).ItIs(x => x.FirstName + " " + x.LastName);
        }

        public string UpdateName(Contact contact)
        {
            return contact.FirstName + " " + contact.LastName;
        }
    }
}
